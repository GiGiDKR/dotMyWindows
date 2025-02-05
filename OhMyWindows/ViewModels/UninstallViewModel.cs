using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OhMyWindows.Services;
using OhMyWindows.Messages;
using OhMyWindows.Models;

namespace OhMyWindows.ViewModels;

public partial class UninstallViewModel : ObservableRecipient
{
    private readonly ProgramService _programService;
    private readonly RegistryService _registryService;


    // Propriétés pour les programmes
    private ObservableCollection<InstalledProgram> _allPrograms = new();
    [ObservableProperty]
    private ObservableCollection<InstalledProgram> installedPrograms = new();

    [ObservableProperty]
    private IList<InstalledProgram> selectedPrograms = new List<InstalledProgram>();

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

    [ObservableProperty]
    private bool sortDescending = false;

    public IEnumerable<string> SortOptions => new[]
    {
        "Nom",
        "Éditeur",
        "Date d'installation",
        "Taille"
    };

    private string FormatSize(long sizeInKb)
    {
        if (sizeInKb >= 1024 * 1024) // Plus de 1 Go
        {
            return $"{sizeInKb / (1024.0 * 1024.0):F2} Go";
        }
        else if (sizeInKb >= 1024) // Plus de 1 Mo
        {
            return $"{sizeInKb / 1024.0:F2} Mo";
        }
        return $"{sizeInKb} Ko";
    }

    public IRelayCommand ToggleSortOrderCommand { get; }

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

        // Créer les expressions de tri
        var sortExp = SortOption switch
        {
            "Nom" => (Func<InstalledProgram, string>)(p => p.Name),
            "Éditeur" => p => p.Publisher ?? "",
            "Date d'installation" => p => p.InstallDate ?? "",
            "Taille" => p => p.EstimatedSize.ToString("D20"),
            _ => p => p.Name
        };

        // Appliquer le tri avec l'ordre
        filtered = SortDescending ? filtered.OrderByDescending(sortExp) : filtered.OrderBy(sortExp);

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
    public IAsyncRelayCommand UninstallSelectedCommand { get; private set; }
    public IAsyncRelayCommand InstallSelectedCommand { get; private set; }
    public IAsyncRelayCommand ConfirmUninstallCommand { get; private set; }
    public IAsyncRelayCommand RefreshProgramsCommand { get; }

    public UninstallViewModel(ProgramService programService, RegistryService registryService)

    {
        _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));

        // Initialisation des commandes
        InstallSelectedCommand = new AsyncRelayCommand(InstallSelectedAsync);
        UninstallSelectedCommand = new AsyncRelayCommand(ShowUninstallConfirmationAsync);
        ConfirmUninstallCommand = new AsyncRelayCommand(UninstallSelectedAsync);
        RefreshProgramsCommand = new AsyncRelayCommand(LoadInstalledProgramsAsync);
        ToggleSortOrderCommand = new RelayCommand(() => 
        {
            SortDescending = !SortDescending;
            UpdateFilteredPrograms();
        });

        // Chargement initial des programmes
        _ = LoadInstalledProgramsAsync();
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
        if (!SelectedPrograms.Any()) return;

        WeakReferenceMessenger.Default.Send(new ShowUninstallConfirmationMessage());
    }

    private async Task UninstallSelectedAsync()
    {
        if (!SelectedPrograms.Any()) return;

        IsError = false;
        ErrorMessage = string.Empty;

        try
        {
            IsInstalling = true;
            var total = SelectedPrograms.Count;
            var current = 0;

            foreach (var program in SelectedPrograms)
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
