using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using OhMyWindows.Models;
using OhMyWindows.Services;
using OhMyWindows.Contracts.Services;
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

public partial class InstallPackagesViewModel : ObservableRecipient
{
    private readonly InstallationService _installationService;
    private readonly IInstalledPackagesService _installedPackagesService;
    private SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<Func<CancellationToken, Task>> _taskQueue;
    private CancellationTokenSource? _cancellationTokenSource;

    private const int DefaultMaxParallelTasks = 3;
    private const int MinParallelTasks = 1;
    private const int MaxParallelTasksLimit = 10;
    private readonly string _logPath;
    private static readonly object _lockObj = new();
    private readonly DispatcherQueue _dispatcherQueue;
    private DispatcherQueueTimer? _statusTimer;
    private DispatcherQueueTimer? _enableVerifyTimer;

    private ObservableCollection<CategoryViewModel> _categories;
    public ObservableCollection<CategoryViewModel> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    private ObservableCollection<PackageViewModel> _allPackages;
    public ObservableCollection<PackageViewModel> AllPackages
    {
        get => _allPackages;
        set => SetProperty(ref _allPackages, value);
    }
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value, nameof(IsLoading));
    }

    private bool _isProcessing;
    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetProperty(ref _isProcessing, value);
    }

    private bool _isChecking;
    public bool IsChecking
    {
        get => _isChecking;
        set => SetProperty(ref _isChecking, value);
    }

    private bool _isInstalling;
    public bool IsInstalling
    {
        get => _isInstalling;
        set => SetProperty(ref _isInstalling, value, nameof(IsInstalling));
    }

    private double _progressValue;
    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    private bool _isIndeterminate;
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set => SetProperty(ref _isIndeterminate, value);
    }

    private string _statusText;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private string _permanentStatusText = string.Empty;
    public string PermanentStatusText
    {
        get => _permanentStatusText;
        set => SetProperty(ref _permanentStatusText, value);
    }

    private bool _canCheck = true;
    public bool CanCheck
    {
        get => _canCheck;
        set => SetProperty(ref _canCheck, value);
    }

    private bool _canInstall = true;
    public bool CanInstall
    {
        get => _canInstall;
        set => SetProperty(ref _canInstall, value);
    }

    private bool _canCancel;
    public bool CanCancel
    {
        get => _canCancel;
        set => SetProperty(ref _canCancel, value);
    }

    private bool _canStop;
    public bool CanStop
    {
        get => _canStop;
        set => SetProperty(ref _canStop, value);
    }

    private bool _isAllExpanded;
    public bool IsAllExpanded
    {
        get => _isAllExpanded;
        set => SetProperty(ref _isAllExpanded, value);
    }

    private bool _isAllSelected;
    public bool IsAllSelected
    {
        get => _isAllSelected;
        set => SetProperty(ref _isAllSelected, value);
    }

    private bool _isVerifyChecked;
    public bool IsVerifyChecked
    {
        get => _isVerifyChecked;
        set => SetProperty(ref _isVerifyChecked, value);
    }

    private string _verifyButtonText = "Vérifier";
    public string VerifyButtonText
    {
        get => _verifyButtonText;
        set => SetProperty(ref _verifyButtonText, value);
    }

    private string _installButtonText = "Installer";
    public string InstallButtonText
    {
        get => _installButtonText;
        set => SetProperty(ref _installButtonText, value);
    }

    private int _maxParallelTasks = DefaultMaxParallelTasks;
    public int MaxParallelTasks
    {
        get => _maxParallelTasks;
        set => SetProperty(ref _maxParallelTasks, value);
    }

    public InstallPackagesViewModel(InstallationService installationService, IInstalledPackagesService installedPackagesService)
    {
        try
        {
            _installationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
            _installedPackagesService = installedPackagesService ?? throw new ArgumentNullException(nameof(installedPackagesService));
            _semaphore = new SemaphoreSlim(DefaultMaxParallelTasks);
            _taskQueue = new ConcurrentQueue<Func<CancellationToken, Task>>();
            _categories = new ObservableCollection<CategoryViewModel>();
            _allPackages = new ObservableCollection<PackageViewModel>();
            _statusText = "Prêt";
            _permanentStatusText = "Prêt";
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
        catch (Exception ex)
        {
            LogToFile($"Erreur dans le constructeur : {ex}");
            throw;
        }
    }

    public ICommand InitializeCommand { get; }
    public ICommand CheckStatusCommand { get; }
    public ICommand StopCheckCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand InstallSelectedCommand { get; }

    private void OnMaxParallelTasksChanged(int value)
    {
        if (value < MinParallelTasks)
            _maxParallelTasks = MinParallelTasks;
        else if (value > MaxParallelTasksLimit)
            _maxParallelTasks = MaxParallelTasksLimit;
        else
            _maxParallelTasks = value;

        _semaphore?.Dispose();
        _semaphore = new SemaphoreSlim(_maxParallelTasks);
    }

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

    public async Task InitializeAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        StatusText = "Chargement des packages...";

        try
        {
            LogToFile("Début de l'initialisation");

            // Vérifier les services
            if (_installationService == null)
            {
                throw new InvalidOperationException("Le service d'installation n'est pas initialisé");
            }

            if (_installedPackagesService == null)
            {
                throw new InvalidOperationException("Le service des packages installés n'est pas initialisé");
            }

            // Vider les collections existantes
            AllPackages.Clear();
            Categories.Clear();

            // Charger les packages
            var packages = await _installationService.GetPackagesAsync();
            if (packages == null)
            {
                throw new InvalidOperationException("Impossible de charger la liste des packages");
            }

            // Créer les ViewModels de packages
            var packageViewModels = new List<PackageViewModel>();
            foreach (var package in packages)
            {
                try
                {
                    var isInstalled = await _installedPackagesService.IsPackageInstalledAsync(package.Id);
                    var packageViewModel = new PackageViewModel(package, isInstalled);
                    packageViewModels.Add(packageViewModel);
                }
                catch (Exception ex)
                {
                    LogToFile($"Erreur lors du chargement du package {package.Id}: {ex.Message}");
                    // Continuer avec le package suivant
                }
            }

            // Ajouter les packages à la collection
            foreach (var packageViewModel in packageViewModels)
            {
                AllPackages.Add(packageViewModel);
            }

            // Grouper par catégorie
            var groupedPackages = packageViewModels
                .GroupBy(p => p.Category)
                .OrderBy(g => g.Key);

            foreach (var group in groupedPackages)
            {
                var categoryViewModel = new CategoryViewModel(group.Key, group.ToList());
                Categories.Add(categoryViewModel);
            }

            LogToFile("Initialisation terminée avec succès");
            StatusText = "Prêt";
        }
        catch (Exception ex)
        {
            LogToFile($"Erreur lors de l'initialisation: {ex}");
            StatusText = $"Erreur lors du chargement : {ex.Message}";
            // Ne pas relancer l'exception pour éviter le plantage
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AnimateProgressValueAsync(double startValue, double endValue, int durationMs = 500)
    {
        var stepCount = 10; // Nombre d'étapes pour l'animation
        var stepDuration = durationMs / stepCount;
        var valueIncrement = (endValue - startValue) / stepCount;

        for (var i = 0; i < stepCount; i++)
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
        StatusText = "Vérification de l'état d'installation...";
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
                                StatusText = $"Vérification de l'état d'installation ({newCount}/{totalCount})";
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

                if (tasks.Count >= _maxParallelTasks)
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
        catch (Exception ex)
        {
            if (!(ex is OperationCanceledException || ex is TaskCanceledException))
            {
                var installedCount = AllPackages.Count(p => p.IsInstalled);
                var totalPackages = AllPackages.Count;
                var permanentStatus = $"Programmes installés : {installedCount}/{totalPackages}";
                ShowTemporaryStatus("Vérification annulée", permanentStatus);
            }
        }
        finally
        {
            if (!_cancellationTokenSource?.IsCancellationRequested ?? false)
            {
                IsChecking = false;
                IsProcessing = false;
                IsVerifyChecked = false;
                ProgressValue = 0;
                EnableVerifyAfterDelay();
            }
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

                // Calculer immédiatement le statut final pour éviter le message d'erreur
                var installedCount = AllPackages.Count(p => p.IsInstalled);
                var totalPackages = AllPackages.Count;
                var permanentStatus = $"Programmes installés : {installedCount}/{totalPackages}";

                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (IsInstalling)
                    {
                        ShowTemporaryStatus("Installation annulée", permanentStatus);
                    }
                    else
                    {
                        ShowTemporaryStatus("Vérification annulée", permanentStatus);
                    }
                });
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

            // Réinitialiser immédiatement l'interface
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsChecking = false;
                IsInstalling = false;
                IsProcessing = false;
                CanInstall = true;
                CanCancel = false;
                IsIndeterminate = false;
                ProgressValue = 0;
            });

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
            var alreadyInstalledPackages = new ConcurrentBag<PackageViewModel>();
            var packagesToInstall = new ConcurrentBag<PackageViewModel>();
            var totalSelected = selectedPackages.Count;
            var processedCount = 0;

            StatusText = $"Vérification de l'état d'installation (0/{totalSelected})";

            var verificationTasks = new List<Task>();
            foreach (var package in selectedPackages)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                var task = Task.Run(async () =>
                {
                    await _semaphore.WaitAsync(_cancellationTokenSource.Token);
                    try
                    {
                        if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                        var isAlreadyInstalled = await _installationService.IsPackageInstalledAsync(package.Package);
                        
                        if (!_cancellationTokenSource.Token.IsCancellationRequested)
                        {
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

                            var newCount = Interlocked.Increment(ref processedCount);
                            _dispatcherQueue.TryEnqueue(() =>
                            {
                                StatusText = $"Vérification de l'état d'installation ({newCount}/{totalSelected})";
                            });
                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, _cancellationTokenSource.Token);

                verificationTasks.Add(task);

                if (verificationTasks.Count >= _maxParallelTasks)
                {
                    await Task.WhenAny(verificationTasks);
                    verificationTasks.RemoveAll(t => t.IsCompleted);
                }
            }

            await Task.WhenAll(verificationTasks);

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

                StatusText = $"Installation des programmes (0/{totalToInstall})";

                var installationTasks = new List<Task>();
                foreach (var package in packagesToInstall)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    var task = Task.Run(async () =>
                    {
                        await _semaphore.WaitAsync(_cancellationTokenSource.Token);
                        try
                        {
                            if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                            if (await _installationService.InstallPackageAsync(package.Package, _cancellationTokenSource.Token))
                            {
                                Interlocked.Increment(ref newlyInstalledCount);
                                await _installedPackagesService.SetPackageInstalledAsync(package.Package.Id, true);
                                _dispatcherQueue.TryEnqueue(() =>
                                {
                                    package.IsInstalled = true;
                                    package.IsSelected = false;
                                });
                            }

                            var newCount = Interlocked.Increment(ref processedCount);
                            _dispatcherQueue.TryEnqueue(() =>
                            {
                                StatusText = $"Installation des programmes ({newCount}/{totalToInstall})";
                            });
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }, _cancellationTokenSource.Token);

                    installationTasks.Add(task);

                    if (installationTasks.Count >= _maxParallelTasks)
                    {
                        await Task.WhenAny(installationTasks);
                        installationTasks.RemoveAll(t => t.IsCompleted);
                    }
                }

                await Task.WhenAll(installationTasks);

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
        catch (Exception ex)
        {
            if (!(ex is OperationCanceledException || ex is TaskCanceledException))
            {
                await Task.Delay(100);
                var installedCount = AllPackages.Count(p => p.IsInstalled);
                var totalPackages = AllPackages.Count;
                ShowTemporaryStatus($"Erreur : {ex.Message}", $"Programmes installés : {installedCount}/{totalPackages}");
            }
        }
        finally
        {
            if (!_cancellationTokenSource?.IsCancellationRequested ?? false)
            {
                IsIndeterminate = false;
                CanInstall = true;
                CanCancel = false;
                IsInstalling = false;
                IsProcessing = false;
                ProgressValue = 0;
            }
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


    private void OnIsVerifyCheckedChanged(bool value)
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
