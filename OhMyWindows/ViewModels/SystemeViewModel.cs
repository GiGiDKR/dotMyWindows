using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OhMyWindows.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OhMyWindows.ViewModels;

public partial class SystemeViewModel : ObservableObject
{
    private readonly RegistryService _registryService;
    private readonly Dictionary<string, bool> _modifiedSettings;
    private bool _isSelectAll;

    [ObservableProperty]
    private string selectAllButtonText = "Tout sélectionner";

    [ObservableProperty]
    private bool taskbarAlignmentCenter;

    [ObservableProperty]
    private bool taskbarEndTask;

    [ObservableProperty]
    private int searchboxTaskbarMode;

    [ObservableProperty]
    private bool taskViewButton;

    [ObservableProperty]
    private bool bingSearch;

    [ObservableProperty]
    private bool webSearch;

    [ObservableProperty]
    private bool subscribedContent;

    [ObservableProperty]
    private bool offlineMaps;

    [ObservableProperty]
    private bool appIconsThumbnails;

    [ObservableProperty]
    private bool automaticFolderDiscovery;

    [ObservableProperty]
    private bool compactView;

    [ObservableProperty]
    private bool fileExtension;

    [ObservableProperty]
    private bool hiddenFiles;

    [ObservableProperty]
    private bool classicContextMenu;

    [ObservableProperty]
    private bool autoHideTaskbar;

    [ObservableProperty]
    private bool themeContextMenu;

    [ObservableProperty]
    private bool mouseAcceleration;

    [ObservableProperty]
    private bool microsoftCopilot;

    [ObservableProperty]
    private bool backgroundApps;

    [ObservableProperty]
    private bool devMode;

    [ObservableProperty]
    private bool hibernation;

    [ObservableProperty]
    private bool telemetry;

    [ObservableProperty]
    private bool hasModifications;

    public SystemeViewModel(RegistryService registryService)
    {
        _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
        _modifiedSettings = new Dictionary<string, bool>();
        InitializePropertyChangedHandlers();
        LoadSettingsAsync();
    }

    private void InitializePropertyChangedHandlers()
    {
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(HasModifications) && 
                e.PropertyName != nameof(SelectAllButtonText) &&
                e.PropertyName != null)
            {
                try
                {
                    var value = GetPropertyValue<bool>(e.PropertyName);
                    _modifiedSettings[e.PropertyName] = value;
                    HasModifications = _modifiedSettings.Count > 0;
                }
                catch (InvalidCastException)
                {
                    // Ignorer les propriétés qui ne sont pas des booléens (comme SearchboxTaskbarMode)
                }
            }
        };
    }

    private T GetPropertyValue<T>(string propertyName)
    {
        var property = GetType().GetProperty(propertyName);
        if (property == null)
        {
            throw new ArgumentException($"Propriété non trouvée : {propertyName}");
        }
        return (T)property.GetValue(this)!;
    }

    private async void LoadSettingsAsync()
    {
        try
        {
            TaskbarAlignmentCenter = await _registryService.IsSettingEnabled("Barre des tache", "Taskbar Al - Center");
            TaskbarEndTask = await _registryService.IsSettingEnabled("Barre des tache", "Enable Taskbar End Task");
            TaskViewButton = await _registryService.IsSettingEnabled("Barre des tache", "Show Task View Button");
            BingSearch = await _registryService.IsSettingEnabled("Confidentialite et Recherche", "BingSearchEnabled-On");
            WebSearch = await _registryService.IsSettingEnabled("Confidentialite et Recherche", "Enable Web Search");
            // ... Charger les autres paramètres
            
            _modifiedSettings.Clear();
            HasModifications = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des paramètres : {ex.Message}");
            // TODO: Afficher un message d'erreur à l'utilisateur
        }
    }

    [RelayCommand]
    private void ToggleAllSettings()
    {
        _isSelectAll = !_isSelectAll;
        SelectAllButtonText = _isSelectAll ? "Tout désélectionner" : "Tout sélectionner";

        TaskbarAlignmentCenter = _isSelectAll;
        TaskbarEndTask = _isSelectAll;
        TaskViewButton = _isSelectAll;
        BingSearch = _isSelectAll;
        WebSearch = _isSelectAll;
        SubscribedContent = _isSelectAll;
        OfflineMaps = _isSelectAll;
        AppIconsThumbnails = _isSelectAll;
        AutomaticFolderDiscovery = _isSelectAll;
        CompactView = _isSelectAll;
        FileExtension = _isSelectAll;
        HiddenFiles = _isSelectAll;
        ClassicContextMenu = _isSelectAll;
        AutoHideTaskbar = _isSelectAll;
        ThemeContextMenu = _isSelectAll;
        MouseAcceleration = _isSelectAll;
        MicrosoftCopilot = _isSelectAll;
        BackgroundApps = _isSelectAll;
        DevMode = _isSelectAll;
        Hibernation = _isSelectAll;
        Telemetry = _isSelectAll;
    }

    [RelayCommand]
    private async Task ApplyModifiedSettings()
    {
        try
        {
            foreach (var setting in _modifiedSettings)
            {
                await _registryService.ApplyRegFile(GetCategoryForSetting(setting.Key), setting.Key, setting.Value);
            }
            _modifiedSettings.Clear();
            HasModifications = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'application des modifications : {ex.Message}");
            // TODO: Afficher un message d'erreur à l'utilisateur
            throw;
        }
    }

    private string GetCategoryForSetting(string setting)
    {
        return setting switch
        {
            "TaskbarAlignmentCenter" => "Barre des tache",
            "TaskbarEndTask" => "Barre des tache",
            "TaskViewButton" => "Barre des tache",
            "BingSearch" => "Confidentialite et Recherche",
            "WebSearch" => "Confidentialite et Recherche",
            "SubscribedContent" => "Confidentialite et Recherche",
            "OfflineMaps" => "Confidentialite et Recherche",
            "AppIconsThumbnails" => "Explorateur de fichiers",
            "AutomaticFolderDiscovery" => "Explorateur de fichiers",
            "CompactView" => "Explorateur de fichiers",
            "FileExtension" => "Explorateur de fichiers",
            "HiddenFiles" => "Explorateur de fichiers",
            "ClassicContextMenu" => "Menu Contextuel",
            "AutoHideTaskbar" => "Menu Contextuel",
            "ThemeContextMenu" => "Menu Contextuel",
            "MouseAcceleration" => "Systeme et Performance",
            "MicrosoftCopilot" => "Systeme et Performance",
            "BackgroundApps" => "Systeme et Performance",
            "DevMode" => "Systeme et Performance",
            "Hibernation" => "Systeme et Performance",
            "Telemetry" => "Systeme et Performance",
            _ => throw new ArgumentException($"Catégorie inconnue pour le paramètre : {setting}")
        };
    }

    [RelayCommand]
    private async Task ApplySettings()
    {
        // TODO: Appliquer les modifications au registre
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ResetSettings()
    {
        // TODO: Réinitialiser tous les paramètres
        await Task.CompletedTask;
    }
} 