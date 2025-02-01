using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using OhMyWindows.Models;
using OhMyWindows.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;

namespace OhMyWindows.ViewModels;

public partial class InstallationViewModel : ObservableRecipient
{
    private readonly InstallationService _installationService;
    private readonly InstalledPackagesService _installedPackagesService;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<Func<CancellationToken, Task>> _taskQueue;
    private CancellationTokenSource? _cancellationTokenSource;
    private const int DefaultMaxParallelTasks = 3;
    private readonly string _logPath;
    private static readonly object _lockObj = new object();
    private readonly DispatcherQueue _dispatcherQueue;
    private DispatcherQueueTimer? _statusTimer;
    private DispatcherQueueTimer? _enableVerifyTimer;

    [ObservableProperty]
    private ObservableCollection<CategoryViewModel> _categories;

    [ObservableProperty]
    private ObservableCollection<PackageViewModel> _allPackages;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private bool _isChecking;

    [ObservableProperty]
    private bool _isInstalling;

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private bool _isIndeterminate;

    [ObservableProperty]
    private string _statusText;

    [ObservableProperty]
    private string _permanentStatusText;

    [ObservableProperty]
    private bool _canCheck = true;

    [ObservableProperty]
    private bool _canInstall = true;

    [ObservableProperty]
    private bool _canCancel;

    [ObservableProperty]
    private bool _canStop;

    [ObservableProperty]
    private bool _isAllExpanded;

    [ObservableProperty]
    private bool _isAllSelected;

    [ObservableProperty]
    private bool _isVerifyChecked;

    [ObservableProperty]
    private string _verifyButtonText = "Vérifier";

    [ObservableProperty]
    private string _installButtonText = "Installer";

    public InstallationViewModel(InstallationService installationService, InstalledPackagesService installedPackagesService)
    {
        _installationService = installationService;
        _installedPackagesService = installedPackagesService;
        _semaphore = new SemaphoreSlim(DefaultMaxParallelTasks);
        _taskQueue = new ConcurrentQueue<Func<CancellationToken, Task>>();
        _categories = new ObservableCollection<CategoryViewModel>();
        _allPackages = new ObservableCollection<PackageViewModel>();
        _statusText = "Prêt";
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installation_viewmodel.log");
        LogToFile("ViewModel initialisé");

        InitializeCommand = new AsyncRelayCommand(InitializeAsync);
        CheckStatusCommand = new AsyncRelayCommand(CheckStatusAsync);
        StopCheckCommand = new RelayCommand(StopChecking);
        ResetCommand = new RelayCommand(Reset);
        SelectAllCommand = new RelayCommand(SelectAll);
        InstallSelectedCommand = new AsyncRelayCommand(InstallSelectedAsync);
    }

    public ICommand InitializeCommand { get; }
    public ICommand CheckStatusCommand { get; }
    public ICommand StopCheckCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand InstallSelectedCommand { get; }

    private void LogToFile(string message)
    {
        try
        {
            lock (_lockObj)
            {
                File.AppendAllText(_logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Ignorer les erreurs de logging
        }
    }

    private void ShowTemporaryStatus(string temporaryMessage, string permanentMessage)
    {
        _statusTimer?.Stop();
        
        StatusText = temporaryMessage;
        PermanentStatusText = permanentMessage;

        _statusTimer = _dispatcherQueue.CreateTimer();
        _statusTimer.Interval = TimeSpan.FromSeconds(2);
        _statusTimer.Tick += (s, e) =>
        {
            StatusText = PermanentStatusText;
            _statusTimer?.Stop();
        };
        _statusTimer.Start();
    }

    private async Task InitializeAsync()
    {
        IsLoading = true;
        StatusText = "Chargement des packages...";
        LogToFile("Début de l'initialisation");

        try
        {
            await _installationService.InitializeAsync();
            var packages = await _installationService.GetPackagesAsync();

            Categories.Clear();
            AllPackages.Clear();

            // Créer les PackageViewModels une seule fois et les réutiliser
            var packageViewModels = packages
                .Select(p => new PackageViewModel(p))
                .OrderBy(p => p.Package.Name)
                .ToList();

            // Remplir AllPackages
            foreach (var packageViewModel in packageViewModels)
            {
                AllPackages.Add(packageViewModel);
            }

            // Grouper et remplir les catégories
            var groupedPackages = packageViewModels
                .GroupBy(p => p.Package.Category)
                .OrderBy(g => g.Key);

            foreach (var group in groupedPackages)
            {
                var categoryViewModel = new CategoryViewModel(group.Key, group.ToList());
                Categories.Add(categoryViewModel);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Erreur lors du chargement : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            if (StatusText == "Chargement des packages...")
            {
                StatusText = "Prêt";
            }
        }
    }

    private async Task AnimateProgressValueAsync(double startValue, double endValue, int durationMs = 500)
    {
        var stepCount = 20; // Nombre d'étapes pour l'animation
        var stepDuration = durationMs / stepCount;
        var valueIncrement = (endValue - startValue) / stepCount;

        for (int i = 0; i < stepCount; i++)
        {
            ProgressValue = startValue + (valueIncrement * i);
            await Task.Delay(stepDuration);
        }
        ProgressValue = endValue;
    }

    private async Task CheckStatusAsync()
    {
        if (IsProcessing && !IsChecking) return;

        IsProcessing = true;
        IsChecking = true;
        StatusText = "Vérification des installations...";
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var packages = AllPackages.Where(p => !p.IsInstalled).ToList();
            var totalCount = packages.Count;
            var processedCount = 0;
            var currentProgress = 0.0;

            var tasks = new List<Task>();
            foreach (var package in packages)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                var task = Task.Run(async () =>
                {
                    await _semaphore.WaitAsync(_cancellationTokenSource.Token);
                    try
                    {
                        if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                        var isInstalled = await _installationService.IsPackageInstalledAsync(package.Package);
                        
                        if (!_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            _dispatcherQueue.TryEnqueue(async () =>
                            {
                                package.IsInstalled = isInstalled;
                                if (isInstalled)
                                {
                                    package.IsSelected = false;
                                }
                                var newCount = Interlocked.Increment(ref processedCount);
                                StatusText = $"Vérification de {package.Name}... ({newCount}/{totalCount})";
                                var nextProgress = (double)newCount / totalCount * 100;
                                await AnimateProgressValueAsync(currentProgress, nextProgress);
                                currentProgress = nextProgress;
                            });
                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, _cancellationTokenSource.Token);

                tasks.Add(task);

                if (tasks.Count >= DefaultMaxParallelTasks)
                {
                    await Task.WhenAny(tasks);
                    tasks.RemoveAll(t => t.IsCompleted);
                }
            }

            await Task.WhenAll(tasks);
            
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var installedCount = AllPackages.Count(p => p.IsInstalled);
                var totalPackages = AllPackages.Count;
                var permanentStatus = $"Programmes installés : {installedCount}/{totalPackages}";
                ShowTemporaryStatus("Vérification terminée", permanentStatus);
            }
        }
        catch (OperationCanceledException)
        {
            var installedCount = AllPackages.Count(p => p.IsInstalled);
            var totalPackages = AllPackages.Count;
            var permanentStatus = $"Programmes installés : {installedCount}/{totalPackages}";
            ShowTemporaryStatus("Vérification annulée", permanentStatus);
        }
        catch (Exception)
        {
            var installedCount = AllPackages.Count(p => p.IsInstalled);
            var totalPackages = AllPackages.Count;
            var permanentStatus = $"Programmes installés : {installedCount}/{totalPackages}";
            ShowTemporaryStatus("Vérification annulée", permanentStatus);
        }
        finally
        {
            IsChecking = false;
            IsProcessing = false;
            IsVerifyChecked = false;
            EnableVerifyAfterDelay();
        }
    }

    private void StopChecking()
    {
        try
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                StatusText = "Annulation en cours...";
            }
        }
        finally
        {
            try
            {
                _cancellationTokenSource?.Dispose();
            }
            catch
            {
                // Ignorer les erreurs de disposition
            }
            _cancellationTokenSource = null;
            IsChecking = false;
            IsInstalling = false;
            IsProcessing = false;
            CanInstall = true;
            CanCancel = false;
            ProgressValue = 0;
            EnableVerifyAfterDelay();
        }
    }

    private async Task InstallSelectedAsync()
    {
        if (IsProcessing) return;

        var selectedPackages = AllPackages.Where(p => p.IsSelected && !p.IsInstalled).ToList();
        if (!selectedPackages.Any())
        {
            StatusText = "Aucun package sélectionné à installer";
            return;
        }

        IsProcessing = true;
        IsInstalling = true;
        IsIndeterminate = true;
        CanInstall = false;
        CanCancel = true;
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            // Première étape : vérifier tous les packages sélectionnés
            var alreadyInstalledPackages = new List<PackageViewModel>();
            var packagesToInstall = new List<PackageViewModel>();
            var totalSelected = selectedPackages.Count;
            var processedCount = 0;

            foreach (var package in selectedPackages)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                StatusText = $"Vérification du package {package.Name}";
                var isAlreadyInstalled = await _installationService.IsPackageInstalledAsync(package.Package);
                processedCount++;
                ProgressValue = (double)processedCount / totalSelected * 100;

                if (isAlreadyInstalled)
                {
                    alreadyInstalledPackages.Add(package);
                    await _installedPackagesService.SetPackageInstalledAsync(package.Package.Id, true);
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        package.IsInstalled = true;
                        package.IsSelected = false;
                    });
                }
                else
                {
                    packagesToInstall.Add(package);
                }
            }

            // Attendre que toutes les mises à jour de l'interface soient terminées
            await Task.Delay(100);
            var installedCount = AllPackages.Count(p => p.IsInstalled);
            var totalPackages = AllPackages.Count;
            var permanentStatus = $"Programmes installés : {installedCount}/{totalPackages}";

            // Si tous les packages sont déjà installés
            if (alreadyInstalledPackages.Count > 0 && packagesToInstall.Count == 0)
            {
                ShowTemporaryStatus($"Programme(s) déjà installé(s) : {alreadyInstalledPackages.Count}", permanentStatus);
                return;
            }

            // Deuxième étape : installer les packages non installés
            if (packagesToInstall.Count > 0)
            {
                var newlyInstalledCount = 0;
                processedCount = 0;
                var totalToInstall = packagesToInstall.Count;
                IsIndeterminate = true;

                foreach (var package in packagesToInstall)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    StatusText = $"Installation du package {package.Name}";
                    if (await _installationService.InstallPackageAsync(package.Package, _cancellationTokenSource.Token))
                    {
                        newlyInstalledCount++;
                        await _installedPackagesService.SetPackageInstalledAsync(package.Package.Id, true);
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            package.IsInstalled = true;
                            package.IsSelected = false;
                        });
                    }
                    processedCount++;
                }

                // Attendre que toutes les mises à jour de l'interface soient terminées
                await Task.Delay(100);
                installedCount = AllPackages.Count(p => p.IsInstalled);
                permanentStatus = $"Programmes installés : {installedCount}/{totalPackages}";

                if (alreadyInstalledPackages.Count > 0 && newlyInstalledCount > 0)
                {
                    ShowTemporaryStatus(
                        $"Programme(s) déjà installé(s) : {alreadyInstalledPackages.Count} - Programme(s) installé(s) : {newlyInstalledCount}",
                        permanentStatus);
                }
                else if (newlyInstalledCount > 0)
                {
                    ShowTemporaryStatus($"Programme(s) installé(s) : {newlyInstalledCount}", permanentStatus);
                }
            }
        }
        catch (OperationCanceledException)
        {
            await Task.Delay(100);
            var installedCount = AllPackages.Count(p => p.IsInstalled);
            var totalPackages = AllPackages.Count;
            ShowTemporaryStatus("Installation annulée", $"Programmes installés : {installedCount}/{totalPackages}");
        }
        catch (Exception ex)
        {
            await Task.Delay(100);
            var installedCount = AllPackages.Count(p => p.IsInstalled);
            var totalPackages = AllPackages.Count;
            ShowTemporaryStatus($"Erreur : {ex.Message}", $"Programmes installés : {installedCount}/{totalPackages}");
        }
        finally
        {
            IsIndeterminate = false;
            CanInstall = true;
            CanCancel = false;
            IsInstalling = false;
            IsProcessing = false;
            ProgressValue = 0;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void Reset()
    {
        if (IsProcessing)
        {
            _cancellationTokenSource?.Cancel();
            IsVerifyChecked = false;
            CanCheck = false;
            EnableVerifyAfterDelay();
        }

        foreach (var package in AllPackages)
        {
            package.IsSelected = false;
            package.IsInstalled = false;
        }

        IsProcessing = false;
        IsChecking = false;
        IsInstalling = false;
        IsAllSelected = false;
        CanInstall = true;
        CanCancel = false;
        ProgressValue = 0;
        StatusText = "Réinitialisé";

        try
        {
            _cancellationTokenSource?.Dispose();
        }
        catch
        {
            // Ignorer les erreurs de disposition
        }
        _cancellationTokenSource = null;
    }

    private void SelectAll()
    {
        IsAllSelected = !IsAllSelected;
        foreach (var package in AllPackages.Where(p => !p.IsInstalled))
        {
            package.IsSelected = IsAllSelected;
        }
    }

    [RelayCommand]
    private void ToggleAllExpanded()
    {
        IsAllExpanded = !IsAllExpanded;
        foreach (var category in Categories)
        {
            category.IsExpanded = IsAllExpanded;
        }
    }

    private void EnableVerifyAfterDelay()
    {
        _enableVerifyTimer?.Stop();
        _enableVerifyTimer = _dispatcherQueue.CreateTimer();
        _enableVerifyTimer.Interval = TimeSpan.FromSeconds(2);
        _enableVerifyTimer.Tick += (s, e) =>
        {
            CanCheck = true;
            _enableVerifyTimer?.Stop();
        };
        _enableVerifyTimer.Start();
    }

    [RelayCommand]
    private void CancelOperation()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            StopChecking();
        }
    }

    partial void OnIsVerifyCheckedChanged(bool value)
    {
        if (value)
        {
            CanCheck = true;
            _ = CheckStatusAsync();
        }
        else
        {
            CanCheck = false;
            if (IsProcessing)
            {
                StopChecking();
            }
            else
            {
                EnableVerifyAfterDelay();
            }
        }
    }
}
