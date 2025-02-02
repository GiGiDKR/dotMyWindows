using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OhMyWindows.Services;
using OhMyWindows.Messages;

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
    private ObservableCollection<InstalledProgram> _allPrograms = new();
    private string _searchText = string.Empty;
    private string _sortOption = "Name";

    [ObservableProperty]
    private ObservableCollection<InstalledProgram> installedPrograms = new();

    [ObservableProperty]
    private bool isInstalling;

    [ObservableProperty]
    private string currentOperation = string.Empty;

    [ObservableProperty]
    private double installProgress;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string sortOption = "Name";

    public IEnumerable<string> SortOptions => new[]
    {
        "Nom",
        "Éditeur",
        "Date d'installation",
        "Taille"
    };

    partial void OnSearchTextChanged(string value)
    {
        UpdateFilteredPrograms();
    }

    partial void OnSortOptionChanged(string value)
    {
        UpdateFilteredPrograms();
    }

    private void UpdateFilteredPrograms()
    {
        var filtered = _allPrograms.AsEnumerable();

        // Appliquer le filtre de recherche
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                (p.Publisher?.ToLower().Contains(searchLower) ?? false) ||
                (p.Version?.ToLower().Contains(searchLower) ?? false));
        }

        // Appliquer le tri
        filtered = SortOption switch
        {
            "Nom" => filtered.OrderBy(p => p.Name),
            "Éditeur" => filtered.OrderBy(p => p.Publisher ?? ""),
            "Date d'installation" => filtered.OrderByDescending(p => p.InstallDate ?? ""),
            "Taille" => filtered.OrderByDescending(p => p.EstimatedSize),
            _ => filtered.OrderBy(p => p.Name)
        };

        InstalledPrograms.Clear();
        foreach (var program in filtered)
        {
            InstalledPrograms.Add(program);
        }
    }

    [ObservableProperty]
    private bool isError;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    // Commandes
    public IAsyncRelayCommand ToggleAllSettingsCommand { get; }
    public IAsyncRelayCommand ExpandAllCommand { get; }
    public IAsyncRelayCommand InstallSelectedCommand { get; }
    public IAsyncRelayCommand UninstallSelectedCommand { get; private set; }
    public IAsyncRelayCommand ConfirmUninstallCommand { get; private set; }
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
        UninstallSelectedCommand = new AsyncRelayCommand(ShowUninstallConfirmationAsync);
        ConfirmUninstallCommand = new AsyncRelayCommand(UninstallSelectedAsync);
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

    private async Task ShowUninstallConfirmationAsync()
    {
        var selectedPrograms = InstalledPrograms.Where(p => p.IsSelected).ToList();
        if (!selectedPrograms.Any()) return;

        WeakReferenceMessenger.Default.Send(new ShowUninstallConfirmationMessage());
    }

    private async Task UninstallSelectedAsync()
    {
        var selectedPrograms = InstalledPrograms.Where(p => p.IsSelected).ToList();
        if (!selectedPrograms.Any()) return;

        IsError = false;
        ErrorMessage = string.Empty;

        try
        {
            IsInstalling = true;
            var total = selectedPrograms.Count;
            var current = 0;

            foreach (var program in selectedPrograms)
            {
                CurrentOperation = $"Désinstallation de {program.Name}...";
                InstallProgress = (double)current / total * 100;

                var success = await _programService.UninstallProgramAsync(program);
                if (!success)
                {
                    IsError = true;
                    ErrorMessage = $"Échec de la désinstallation de {program.Name}. Veuillez vérifier les permissions et réessayer.";
                    break;
                }
                current++;
            }

            if (!IsError)
            {
                CurrentOperation = "Désinstallation terminée avec succès !";
                await Task.Delay(2000); // Affiche le message de succès pendant 2 secondes
            }
        }
        catch (Exception ex)
        {
            IsError = true;
            ErrorMessage = $"Une erreur est survenue : {ex.Message}";
        }
        finally
        {
            IsInstalling = false;
            CurrentOperation = string.Empty;
            InstallProgress = 0;
            await LoadInstalledProgramsAsync();
        }
    }

    private async Task LoadInstalledProgramsAsync()
    {
        _allPrograms.Clear();
        InstalledPrograms.Clear();
        var programs = await _programService.GetInstalledProgramsAsync();
        foreach (var program in programs)
        {
            _allPrograms.Add(program);
        }
        UpdateFilteredPrograms();
    }
}
