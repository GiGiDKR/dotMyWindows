using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OhMyWindows.Services;
using System;
using System.Threading.Tasks;

namespace OhMyWindows.ViewModels;

public partial class InstallationViewModel : ObservableRecipient
{
    private readonly PackageManagerInstallationService _installationService;

    private bool isWingetSelected;

    public bool IsWingetSelected
    {
        get => isWingetSelected;
        set => SetProperty(ref isWingetSelected, value);
    }

    private bool isChocolateySelected;

    public bool IsChocolateySelected
    {
        get => isChocolateySelected;
        set => SetProperty(ref isChocolateySelected, value);
    }

    private bool isScoopSelected;

    public bool IsScoopSelected
    {
        get => isScoopSelected;
        set => SetProperty(ref isScoopSelected, value);
    }

    private bool isPipSelected;

    public bool IsPipSelected
    {
        get => isPipSelected;
        set => SetProperty(ref isPipSelected, value);
    }

    // Options WinGet
    private bool isForceSelected;
    public bool IsForceSelected
    {
        get => isForceSelected;
        set => SetProperty(ref isForceSelected, value);
    }

    private bool isDebugSelected;
    public bool IsDebugSelected
    {
        get => isDebugSelected;
        set => SetProperty(ref isDebugSelected, value);
    }

    private bool isWaitSelected;
    public bool IsWaitSelected
    {
        get => isWaitSelected;
        set => SetProperty(ref isWaitSelected, value);
    }

    private bool isNoExitSelected;
    public bool IsNoExitSelected
    {
        get => isNoExitSelected;
        set => SetProperty(ref isNoExitSelected, value);
    }

    private double installProgress;
    public double InstallProgress
    {
        get => installProgress;
        set => SetProperty(ref installProgress, value);
    }

    private string statusMessage = string.Empty;
    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    private bool isInstalling;
    public bool IsInstalling
    {
        get => isInstalling;
        set => SetProperty(ref isInstalling, value);
    }

    public InstallationViewModel(PackageManagerInstallationService installationService)
    {
        _installationService = installationService;
        // Par défaut, WinGet est sélectionné car c'est le gestionnaire de paquets officiel de Windows
        IsWingetSelected = true;
    }

    [RelayCommand]
    private async Task InstallSelectedPackageManagersAsync()
    {
        IsInstalling = true;
        StatusMessage = "Démarrage de l'installation...";
        InstallProgress = 0;

        try
        {
            if (IsWingetSelected)
            {
                StatusMessage = "Installation de WinGet...";
                await _installationService.InstallWinGetAsync(
                    force: IsForceSelected,
                    debug: IsDebugSelected,
                    wait: IsWaitSelected,
                    noExit: IsNoExitSelected);
                InstallProgress += 25;
            }

            if (IsChocolateySelected)
            {
                StatusMessage = "Installation de Chocolatey...";
                await _installationService.InstallChocolateyAsync();
                InstallProgress += 25;
            }

            if (IsScoopSelected)
            {
                StatusMessage = "Installation de Scoop...";
                await _installationService.InstallScoopAsync();
                InstallProgress += 25;
            }

            if (IsPipSelected)
            {
                StatusMessage = "Installation de Pip...";
                await _installationService.InstallPipAsync();
                InstallProgress += 25;
            }

            StatusMessage = "Installation terminée avec succès !";
            InstallProgress = 100;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de l'installation : {ex.Message}";
        }
        finally
        {
            IsInstalling = false;
        }
    }
}
