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
    private readonly ObservableCollection<InstalledProgram> _allPrograms = new();
    private ObservableCollection<InstalledProgram> _installedPrograms = new();
    public ObservableCollection<InstalledProgram> InstalledPrograms
    {
        get => _installedPrograms;
        set => SetProperty(ref _installedPrograms, value);
    }

    private IList<InstalledProgram> _selectedPrograms = new List<InstalledProgram>();
    public IList<InstalledProgram> SelectedPrograms
    {
        get => _selectedPrograms;
        set => SetProperty(ref _selectedPrograms, value);
    }

    private bool isInstalling;
    public bool IsInstalling
    {
        get => isInstalling;
        set => SetProperty(ref isInstalling, value);
    }

    private string currentOperation = string.Empty;
    public string CurrentOperation
    {
        get => currentOperation;
        set => SetProperty(ref currentOperation, value);
    }

    private double installProgress;
    public double InstallProgress
    {
        get => installProgress;
        set => SetProperty(ref installProgress, value);
    }

    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    private string sortOption = "Name";
    public string SortOption
    {
        get => sortOption;
        set => SetProperty(ref sortOption, value);
    }

    private bool sortDescending = false;
    public bool SortDescending
    {
        get => sortDescending;
        set => SetProperty(ref sortDescending, value, nameof(SortDescending));
    }

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

    private void OnSearchTextChanged(string value)
    {
        UpdateFilteredPrograms();
    }
    
    private void OnSortOptionChanged(string value)
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

    private bool isError;
    public bool IsError
    {
        get => isError;
        set => SetProperty(ref isError, value);
    }

    private string errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

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

    private Task ShowUninstallConfirmationAsync()
    {
        if (!SelectedPrograms.Any()) return Task.CompletedTask;

        WeakReferenceMessenger.Default.Send(new ShowUninstallConfirmationMessage());
        return Task.CompletedTask;
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
