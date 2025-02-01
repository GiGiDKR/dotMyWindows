using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using OhMyWindows.Models;
using OhMyWindows.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OhMyWindows.ViewModels;

public partial class InstallationViewModel : ObservableRecipient
{
    private readonly InstallationService _installationService;
    private readonly InstalledPackagesService _installedPackagesService;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<Func<CancellationToken, Task>> _taskQueue;
    private CancellationTokenSource? _cancellationTokenSource;
    private const int DefaultMaxParallelTasks = 3;

    [ObservableProperty]
    private ObservableCollection<CategoryViewModel> _categories;

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

    public InstallationViewModel(InstallationService installationService, InstalledPackagesService installedPackagesService)
    {
        _installationService = installationService;
        _installedPackagesService = installedPackagesService;
        _semaphore = new SemaphoreSlim(DefaultMaxParallelTasks);
        _taskQueue = new ConcurrentQueue<Func<CancellationToken, Task>>();
        _categories = new ObservableCollection<CategoryViewModel>();
        _statusText = "Prêt";

        InitializeCommand = new AsyncRelayCommand(InitializeAsync);
        CheckStatusCommand = new AsyncRelayCommand(CheckStatusAsync, () => CanCheck);
        StopCheckCommand = new RelayCommand(StopCheck, () => CanStop);
        ResetCommand = new RelayCommand(Reset);
        SelectAllCommand = new RelayCommand(SelectAll);
        InstallSelectedCommand = new AsyncRelayCommand(InstallSelectedAsync, () => CanInstall);
    }

    public ICommand InitializeCommand { get; }
    public ICommand CheckStatusCommand { get; }
    public ICommand StopCheckCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand InstallSelectedCommand { get; }

    private async Task InitializeAsync()
    {
        IsLoading = true;
        StatusText = "Chargement des packages...";

        try
        {
            var packages = await _installationService.GetPackagesAsync();
            Categories.Clear();

            var groupedPackages = packages
                .GroupBy(p => p.Category)
                .OrderBy(g => g.Key);

            foreach (var group in groupedPackages)
            {
                var categoryViewModel = new CategoryViewModel(group.Key);
                foreach (var package in group.OrderBy(p => p.Name))
                {
                    categoryViewModel.Packages.Add(new PackageViewModel(package));
                }
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
            StatusText = "Prêt";
        }
    }

    private async Task CheckStatusAsync()
    {
        var allPackages = Categories.SelectMany(c => c.Packages);
        await ProcessTasksAsync(async (package, token) =>
        {
            var isInstalled = await _installationService.IsPackageInstalledAsync(package.Package, token);
            await _installedPackagesService.SetPackageInstalledAsync(package.Package.Id, isInstalled);
            package.IsInstalled = isInstalled;
            if (isInstalled)
            {
                package.IsSelected = false;
            }
        }, "Vérification du statut des packages...", allPackages);
    }

    private async Task InstallSelectedAsync()
    {
        var selectedPackages = Categories
            .SelectMany(c => c.Packages)
            .Where(p => p.IsSelected && !p.IsInstalled)
            .ToList();

        if (!selectedPackages.Any())
        {
            StatusText = "Aucun package sélectionné à installer";
            return;
        }

        await ProcessTasksAsync(async (package, token) =>
        {
            if (await _installationService.InstallPackageAsync(package.Package, token))
            {
                await _installedPackagesService.SetPackageInstalledAsync(package.Package.Id, true);
                package.IsInstalled = true;
                package.IsSelected = false;
            }
        }, "Installation des packages sélectionnés...", selectedPackages);
    }

    private async Task ProcessTasksAsync(Func<PackageViewModel, CancellationToken, Task> action, string statusMessage, IEnumerable<PackageViewModel>? packages = null)
    {
        if (IsProcessing)
        {
            return;
        }

        IsProcessing = true;
        CanCheck = false;
        CanInstall = false;
        CanStop = true;
        StatusText = statusMessage;
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            packages ??= Categories.SelectMany(c => c.Packages);
            var totalCount = packages.Count();
            var processedCount = 0;

            foreach (var package in packages)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                _taskQueue.Enqueue(async (token) =>
                {
                    await _semaphore.WaitAsync(token);
                    try
                    {
                        await action(package, token);
                        Interlocked.Increment(ref processedCount);
                        ProgressValue = (double)processedCount / totalCount * 100;
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });
            }

            var tasks = new List<Task>();
            while (_taskQueue.TryDequeue(out var task))
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                tasks.Add(Task.Run(() => task(_cancellationTokenSource.Token)));
                if (tasks.Count >= DefaultMaxParallelTasks)
                {
                    await Task.WhenAny(tasks);
                    tasks.RemoveAll(t => t.IsCompleted);
                }
            }

            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            StatusText = "Opération annulée";
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
        }
    }

    private void StopCheck()
    {
        _cancellationTokenSource?.Cancel();
    }

    private void Reset()
    {
        foreach (var category in Categories)
        {
            foreach (var package in category.Packages)
            {
                package.IsSelected = false;
            }
        }
    }

    private void SelectAll()
    {
        var allSelected = Categories.All(c => c.Packages.All(p => p.IsSelected));
        foreach (var category in Categories)
        {
            foreach (var package in category.Packages)
            {
                package.IsSelected = !allSelected;
            }
        }
    }
}
