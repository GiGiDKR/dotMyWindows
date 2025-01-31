using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OhMyWindows.Models;
using OhMyWindows.Services;

namespace OhMyWindows.ViewModels;

public partial class InstallationViewModel : ObservableRecipient
{
    private readonly InstallationService _installationService;
    
    [ObservableProperty]
    private ObservableCollection<Package> packages;
    
    [ObservableProperty]
    private bool isCheckingPackages;
    
    [ObservableProperty]
    private bool isBusy;

    public InstallationViewModel(InstallationService installationService)
    {
        _installationService = installationService;
        Packages = new ObservableCollection<Package>();
        LoadPackagesCommand = new AsyncRelayCommand(LoadPackagesAsync);
        InstallSelectedCommand = new AsyncRelayCommand(InstallSelectedPackagesAsync, CanInstallSelectedPackages);
        SelectAllCommand = new RelayCommand(SelectAllPackages);
        CheckInstalledCommand = new AsyncRelayCommand(CheckInstalledPackagesAsync);
        StopCheckingCommand = new RelayCommand(StopChecking);
        ResetCheckedCommand = new RelayCommand(ResetChecked);
        
        _ = LoadPackagesAsync();
    }

    public IAsyncRelayCommand LoadPackagesCommand { get; }
    public IAsyncRelayCommand InstallSelectedCommand { get; }
    public IRelayCommand SelectAllCommand { get; }
    public IAsyncRelayCommand CheckInstalledCommand { get; }
    public IRelayCommand StopCheckingCommand { get; }
    public IRelayCommand ResetCheckedCommand { get; }

    private async Task LoadPackagesAsync()
    {
        IsBusy = true;
        var loadedPackages = await _installationService.GetPackagesAsync();
        Packages.Clear();
        foreach (var package in loadedPackages)
        {
            Packages.Add(package);
        }
        IsBusy = false;
    }

    private bool CanInstallSelectedPackages()
    {
        return Packages.Any(p => p.IsSelected && !p.IsInstalled);
    }

    private async Task InstallSelectedPackagesAsync()
    {
        IsBusy = true;
        var selectedPackages = Packages.Where(p => p.IsSelected && !p.IsInstalled).ToList();
        
        foreach (var package in selectedPackages)
        {
            var success = await _installationService.InstallPackageAsync(package.Id);
            if (success)
            {
                package.IsInstalled = true;
            }
        }
        IsBusy = false;
    }

    private void SelectAllPackages()
    {
        var allSelected = Packages.All(p => p.IsSelected);
        foreach (var package in Packages)
        {
            package.IsSelected = !allSelected;
        }
    }

    private async Task CheckInstalledPackagesAsync()
    {
        IsCheckingPackages = true;
        _installationService.IsChecking = true;

        foreach (var package in Packages)
        {
            if (!_installationService.IsChecking) break;
            
            package.IsChecking = true;
            package.IsInstalled = await _installationService.IsPackageInstalledAsync(package.Id);
            package.IsChecking = false;
        }

        IsCheckingPackages = false;
        _installationService.IsChecking = false;
    }

    private void StopChecking()
    {
        _installationService.IsChecking = false;
        IsCheckingPackages = false;
        foreach (var package in Packages)
        {
            package.IsChecking = false;
        }
    }

    private void ResetChecked()
    {
        foreach (var package in Packages)
        {
            package.IsInstalled = false;
        }
    }
} 