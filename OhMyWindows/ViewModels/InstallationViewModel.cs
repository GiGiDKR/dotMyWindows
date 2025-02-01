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

    [ObservableProperty]
    private ObservableCollection<CategoryViewModel> _categories;

    [ObservableProperty]
    private ObservableCollection<PackageViewModel> _allPackages;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private string _statusText;

    [ObservableProperty]
    private bool _canCheck = true;

    [ObservableProperty]
    private bool _canInstall = true;

    [ObservableProperty]
    private bool _canStop;

    [ObservableProperty]
    private bool _isAllExpanded;

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
        CheckStatusCommand = new AsyncRelayCommand(CheckStatusAsync, () => CanCheck);
        ResetCommand = new RelayCommand(Reset);
        SelectAllCommand = new RelayCommand(SelectAll);
        InstallSelectedCommand = new AsyncRelayCommand(InstallSelectedAsync, () => CanInstall);
    }

    public ICommand InitializeCommand { get; }
    public ICommand CheckStatusCommand { get; }
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

    private async Task CheckStatusAsync()
    {
        if (IsProcessing) return;

        IsProcessing = true;
        CanCheck = false;
        CanInstall = false;
        CanStop = true;
        StatusText = "Vérification des installations...";
        VerifyButtonText = "Arrêter";
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var packages = AllPackages.Where(p => !p.IsInstalled).ToList();  // Ne vérifie que les packages non installés
            var totalCount = packages.Count;
            var processedCount = 0;

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

                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            StatusText = $"Vérification de {package.Name}... ({processedCount + 1}/{totalCount})";
                        });

                        var isInstalled = await _installationService.IsPackageInstalledAsync(package.Package);
                        
                        if (!_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            _dispatcherQueue.TryEnqueue(() =>
                            {
                                package.IsInstalled = isInstalled;
                                if (isInstalled)
                                {
                                    package.IsSelected = false;
                                }
                                Interlocked.Increment(ref processedCount);
                                ProgressValue = (double)processedCount / totalCount * 100;
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
                StatusText = $"Vérification terminée ({processedCount}/{totalCount})";
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Vérification annulée";
        }
        catch (Exception ex)
        {
            StatusText = $"Erreur : {ex.Message}";
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            IsProcessing = false;
            CanCheck = true;
            CanInstall = true;
            CanStop = false;
            ProgressValue = 0;
            VerifyButtonText = "Vérifier";
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
        CanCheck = false;
        CanInstall = false;
        CanStop = true;
        StatusText = "Installation des packages...";
        InstallButtonText = "Annuler";
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var totalCount = selectedPackages.Count;
            var processedCount = 0;

            foreach (var package in selectedPackages)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                StatusText = $"Installation de {package.Name}... ({processedCount + 1}/{totalCount})";

                if (await _installationService.InstallPackageAsync(package.Package, _cancellationTokenSource.Token))
                {
                    await _installedPackagesService.SetPackageInstalledAsync(package.Package.Id, true);
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        package.IsInstalled = true;
                        package.IsSelected = false;
                    });
                    processedCount++;
                    ProgressValue = (double)processedCount / totalCount * 100;
                }
            }

            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                StatusText = processedCount == totalCount 
                    ? "Installation terminée avec succès" 
                    : $"Installation terminée avec {processedCount}/{totalCount} packages installés";
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Installation annulée";
        }
        catch (Exception ex)
        {
            StatusText = $"Erreur : {ex.Message}";
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            IsProcessing = false;
            CanCheck = true;
            CanInstall = true;
            CanStop = false;
            ProgressValue = 0;
            InstallButtonText = "Installer";
        }
    }

    private void Reset()
    {
        foreach (var package in AllPackages)
        {
            package.IsSelected = false;
            package.IsInstalled = false;  // Réactive tous les packages
        }
        StatusText = "Prêt";
        ProgressValue = 0;
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void StopCheck()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
            CanStop = false;  // Désactive le bouton immédiatement
        }
    }

    private void SelectAll()
    {
        var allSelected = AllPackages.All(p => p.IsSelected || p.IsInstalled);
        foreach (var package in AllPackages.Where(p => !p.IsInstalled))
        {
            package.IsSelected = !allSelected;
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
}
