using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OhMyWindows.Services;
using System;
using System.Threading.Tasks;

namespace OhMyWindows.ViewModels;

public partial class InstallationViewModel : ObservableRecipient
{
    private readonly PackageManagerInstallationService _installationService;

    [ObservableProperty]
    private bool isWingetSelected;

    [ObservableProperty]
    private bool isChocolateySelected;

    [ObservableProperty]
    private bool isScoopSelected;

    [ObservableProperty]
    private bool isPipSelected;

    // Options WinGet
    [ObservableProperty]
    private bool isForceSelected;

    [ObservableProperty]
    private bool isDebugSelected;

    [ObservableProperty]
    private bool isWaitSelected;

    [ObservableProperty]
    private bool isNoExitSelected;

    [ObservableProperty]
    private double installProgress;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isInstalling;

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
