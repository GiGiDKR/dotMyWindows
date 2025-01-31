using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OhMyWindows.Services;

namespace OhMyWindows.ViewModels;

public partial class ProgrammesViewModel : ObservableRecipient
{
    private readonly ProgramService _programService;
    private readonly RegistryService _registryService;
    private Dictionary<string, bool> _modifiedSettings;

    [ObservableProperty]
    private bool hasModifications;

    [ObservableProperty]
    private string selectAllButtonText = "Tout sélectionner";

    [ObservableProperty]
    private bool isAllExpanded;

    // Propriétés pour les programmes
    [ObservableProperty]
    private ObservableCollection<InstalledProgram> installedPrograms = new();

    [ObservableProperty]
    private bool isInstalling;

    [ObservableProperty]
    private string currentOperation = string.Empty;

    [ObservableProperty]
    private double installProgress;

    // Commandes
    public IAsyncRelayCommand ToggleAllSettingsCommand { get; }
    public IAsyncRelayCommand ExpandAllCommand { get; }
    public IAsyncRelayCommand InstallSelectedCommand { get; }
    public IAsyncRelayCommand UninstallSelectedCommand { get; }
    public IAsyncRelayCommand RefreshProgramsCommand { get; }

    public ProgrammesViewModel(ProgramService programService, RegistryService registryService)
    {
        _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
        _modifiedSettings = new Dictionary<string, bool>();

        // Initialisation des commandes
        ToggleAllSettingsCommand = new AsyncRelayCommand(ToggleAllSettingsAsync);
        ExpandAllCommand = new AsyncRelayCommand(ToggleExpandAllAsync);
        InstallSelectedCommand = new AsyncRelayCommand(InstallSelectedAsync);
        UninstallSelectedCommand = new AsyncRelayCommand(UninstallSelectedAsync);
        RefreshProgramsCommand = new AsyncRelayCommand(LoadInstalledProgramsAsync);

        // Chargement initial des programmes
        _ = LoadInstalledProgramsAsync();
    }

    private Task ToggleAllSettingsAsync()
    {
        var newState = SelectAllButtonText == "Tout sélectionner";
        SelectAllButtonText = newState ? "Tout désélectionner" : "Tout sélectionner";
        
        foreach (var program in InstalledPrograms)
        {
            program.IsSelected = newState;
        }

        return Task.CompletedTask;
    }

    private Task ToggleExpandAllAsync()
    {
        IsAllExpanded = !IsAllExpanded;
        return Task.CompletedTask;
    }

    private async Task InstallSelectedAsync()
    {
        IsInstalling = true;
        CurrentOperation = "Installation en cours...";
        try
        {
            // TODO: Implémenter la logique d'installation
            await Task.Delay(100); // Pour éviter l'avertissement async
        }
        finally
        {
            IsInstalling = false;
            CurrentOperation = string.Empty;
            await LoadInstalledProgramsAsync();
        }
    }

    private async Task UninstallSelectedAsync()
    {
        var selectedPrograms = InstalledPrograms.Where(p => p.IsSelected).ToList();
        if (!selectedPrograms.Any()) return;

        IsInstalling = true;
        var total = selectedPrograms.Count;
        var current = 0;

        foreach (var program in selectedPrograms)
        {
            CurrentOperation = $"Désinstallation de {program.Name}...";
            InstallProgress = (double)current / total * 100;

            await _programService.UninstallProgramAsync(program);
            current++;
        }

        IsInstalling = false;
        CurrentOperation = string.Empty;
        InstallProgress = 0;
        await LoadInstalledProgramsAsync();
    }

    private async Task LoadInstalledProgramsAsync()
    {
        InstalledPrograms.Clear();
        var programs = await _programService.GetInstalledProgramsAsync();
        foreach (var program in programs)
        {
            InstalledPrograms.Add(program);
        }
    }
} 