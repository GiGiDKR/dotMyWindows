using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
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
            // Barre des tâches
            TaskbarAlignmentCenter = await _registryService.IsSettingEnabled(RegistryKeys.TaskBar.TaskbarAlignment);
            TaskbarEndTask = await _registryService.IsSettingEnabled(RegistryKeys.TaskBar.TaskbarEndTask);
            TaskViewButton = await _registryService.IsSettingEnabled(RegistryKeys.TaskBar.TaskViewButton);
            
            // Confidentialité et Recherche
            BingSearch = await _registryService.IsSettingEnabled(RegistryKeys.Privacy.BingSearch);
            WebSearch = await _registryService.IsSettingEnabled(RegistryKeys.Privacy.WebSearch);
            SubscribedContent = await _registryService.IsSettingEnabled(RegistryKeys.Privacy.SubscribedContent);
            OfflineMaps = await _registryService.IsSettingEnabled(RegistryKeys.Privacy.OfflineMaps);
            
            // Explorateur de fichiers
            AppIconsThumbnails = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.AppIconsThumbnails);
            AutomaticFolderDiscovery = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.AutomaticFolderDiscovery);
            CompactView = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.CompactView);
            FileExtension = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.FileExtension);
            HiddenFiles = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.HiddenFiles);
            
            // Menu Contextuel
            ClassicContextMenu = await _registryService.IsSettingEnabled(RegistryKeys.ContextMenu.ClassicContextMenu);
            AutoHideTaskbar = await _registryService.IsSettingEnabled(RegistryKeys.ContextMenu.AutoHideTaskbar);
            ThemeContextMenu = await _registryService.IsSettingEnabled(RegistryKeys.ContextMenu.ThemeContextMenu);
            
            // Système et Performance
            MouseAcceleration = await _registryService.IsSettingEnabled(RegistryKeys.SystemAndPerformance.MouseAcceleration);
            MicrosoftCopilot = await _registryService.IsSettingEnabled(RegistryKeys.SystemAndPerformance.MicrosoftCopilot);
            BackgroundApps = await _registryService.IsSettingEnabled(RegistryKeys.SystemAndPerformance.BackgroundApps);
            DevMode = await _registryService.IsSettingEnabled(RegistryKeys.SystemAndPerformance.DevMode);
            Hibernation = await _registryService.IsSettingEnabled(RegistryKeys.SystemAndPerformance.Hibernation);
            Telemetry = await _registryService.IsSettingEnabled(RegistryKeys.SystemAndPerformance.Telemetry);
            
            _modifiedSettings.Clear();
            HasModifications = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des paramètres : {ex.Message}");
            await ShowErrorDialog("Erreur lors du chargement des paramètres", ex.Message);
        }
    }

    private async Task ShowErrorDialog(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
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
            // Vérifier les privilèges élevés
            if (await _registryService.RequiresElevatedPrivileges(RegistryKeys.TaskBar.TaskbarAlignment))
            {
                await ShowErrorDialog("Privilèges insuffisants", 
                    "Cette opération nécessite des privilèges administrateur. Veuillez relancer l'application en tant qu'administrateur.");
                return;
            }

            foreach (var setting in _modifiedSettings)
            {
                var registryKey = GetRegistryKeyForSetting(setting.Key);
                await _registryService.SetSetting(registryKey, setting.Value);
            }

            _modifiedSettings.Clear();
            HasModifications = false;
            
            await ShowSuccessDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'application des modifications : {ex.Message}");
            await ShowErrorDialog("Erreur lors de l'application des modifications", ex.Message);
        }
    }

    private RegistryKeys.RegistryKey GetRegistryKeyForSetting(string setting)
    {
        return setting switch
        {
            nameof(TaskbarAlignmentCenter) => RegistryKeys.TaskBar.TaskbarAlignment,
            nameof(TaskbarEndTask) => RegistryKeys.TaskBar.TaskbarEndTask,
            nameof(TaskViewButton) => RegistryKeys.TaskBar.TaskViewButton,
            nameof(BingSearch) => RegistryKeys.Privacy.BingSearch,
            nameof(WebSearch) => RegistryKeys.Privacy.WebSearch,
            nameof(SubscribedContent) => RegistryKeys.Privacy.SubscribedContent,
            nameof(OfflineMaps) => RegistryKeys.Privacy.OfflineMaps,
            nameof(AppIconsThumbnails) => RegistryKeys.FileExplorer.AppIconsThumbnails,
            nameof(AutomaticFolderDiscovery) => RegistryKeys.FileExplorer.AutomaticFolderDiscovery,
            nameof(CompactView) => RegistryKeys.FileExplorer.CompactView,
            nameof(FileExtension) => RegistryKeys.FileExplorer.FileExtension,
            nameof(HiddenFiles) => RegistryKeys.FileExplorer.HiddenFiles,
            nameof(ClassicContextMenu) => RegistryKeys.ContextMenu.ClassicContextMenu,
            nameof(AutoHideTaskbar) => RegistryKeys.ContextMenu.AutoHideTaskbar,
            nameof(ThemeContextMenu) => RegistryKeys.ContextMenu.ThemeContextMenu,
            nameof(MouseAcceleration) => RegistryKeys.SystemAndPerformance.MouseAcceleration,
            nameof(MicrosoftCopilot) => RegistryKeys.SystemAndPerformance.MicrosoftCopilot,
            nameof(BackgroundApps) => RegistryKeys.SystemAndPerformance.BackgroundApps,
            nameof(DevMode) => RegistryKeys.SystemAndPerformance.DevMode,
            nameof(Hibernation) => RegistryKeys.SystemAndPerformance.Hibernation,
            nameof(Telemetry) => RegistryKeys.SystemAndPerformance.Telemetry,
            _ => throw new ArgumentException($"Paramètre inconnu : {setting}")
        };
    }

    private async Task ShowSuccessDialog()
    {
        var dialog = new ContentDialog
        {
            Title = "Modifications appliquées",
            Content = "Les modifications ont été appliquées avec succès.",
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
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