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
    private bool canExpandAll = true;

    [ObservableProperty]
    private bool canCollapseAll = false;

    [ObservableProperty]
    private bool isTaskbarExpanded;

    [ObservableProperty]
    private bool isPrivacyExpanded;

    [ObservableProperty]
    private bool isFileExplorerExpanded;

    [ObservableProperty]
    private bool isContextMenuExpanded;

    [ObservableProperty]
    private bool isSystemExpanded;

    [ObservableProperty]
    private bool taskbarAlignmentCenter;

    [ObservableProperty]
    private bool taskbarEndTask;

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

    private int _searchboxTaskbarMode;
    public int SearchboxTaskbarMode
    {
        get => _searchboxTaskbarMode;
        set
        {
            if (SetProperty(ref _searchboxTaskbarMode, value))
            {
                _modifiedSettings[nameof(SearchboxTaskbarMode)] = true;
                HasModifications = _modifiedSettings.Count > 0;
            }
        }
    }

    private bool _systemFiles;
    public bool SystemFiles
    {
        get => _systemFiles;
        set => SetProperty(ref _systemFiles, value);
    }

    [ObservableProperty]
    private bool copyMoreDetails;

    [ObservableProperty]
    private bool driveLetters;

    [ObservableProperty]
    private bool foldersGroupToThisPC;

    [ObservableProperty]
    private bool gallery;

    [ObservableProperty]
    private bool controlPanelInNavigation;

    [ObservableProperty]
    private bool recycleBinInNavigation;

    [ObservableProperty]
    private bool userFolderInNavigation;

    [ObservableProperty]
    private bool removableDrivesInSidebar;

    [ObservableProperty]
    private bool openFileExplorerToThisPC;

    [ObservableProperty]
    private bool recentDocsHistory;

    [ObservableProperty]
    private bool recentlyAddedApps;

    [ObservableProperty]
    private bool shortcutText;

    [ObservableProperty]
    private bool snapLayouts;

    [ObservableProperty]
    private bool statusBar;

    public SystemeViewModel(RegistryService registryService)
    {
        _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
        _modifiedSettings = new Dictionary<string, bool>();
        InitializePropertyChangedHandlers();
        InitializeExpanderHandlers();
        LoadSettingsAsync();
    }

    private void InitializePropertyChangedHandlers()
    {
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(HasModifications) && 
                e.PropertyName != nameof(SelectAllButtonText) &&
                e.PropertyName != nameof(CanExpandAll) &&
                e.PropertyName != nameof(CanCollapseAll) &&
                !e.PropertyName?.EndsWith("Expanded") == true &&
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

    private void InitializeExpanderHandlers()
    {
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName?.EndsWith("Expanded") == true)
            {
                UpdateExpandCollapseButtonsState();
            }
        };
    }

    private void UpdateExpandCollapseButtonsState()
    {
        var allExpanded = IsTaskbarExpanded && IsPrivacyExpanded && 
                         IsFileExplorerExpanded && IsContextMenuExpanded && 
                         IsSystemExpanded;

        var allCollapsed = !IsTaskbarExpanded && !IsPrivacyExpanded && 
                          !IsFileExplorerExpanded && !IsContextMenuExpanded && 
                          !IsSystemExpanded;

        CanExpandAll = !allExpanded;
        CanCollapseAll = !allCollapsed;
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
            
            // Charger la valeur de SearchboxTaskbarMode
            try
            {
                using var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Search");
                if (regKey != null)
                {
                    var value = regKey.GetValue("SearchboxTaskbarMode");
                    SearchboxTaskbarMode = value != null ? Convert.ToInt32(value) : 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la lecture de SearchboxTaskbarMode : {ex.Message}");
                SearchboxTaskbarMode = 1; // Valeur par défaut
            }
            
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
            SystemFiles = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.SystemFiles);
            
            // Nouvelles options de l'explorateur
            CopyMoreDetails = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.CopyMoreDetails);
            DriveLetters = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.DriveLetters);
            FoldersGroupToThisPC = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.FoldersGroupToThisPC);
            Gallery = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.Gallery);
            ControlPanelInNavigation = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.ControlPanelInNavigation);
            RecycleBinInNavigation = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.RecycleBinInNavigation);
            UserFolderInNavigation = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.UserFolderInNavigation);
            RemovableDrivesInSidebar = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.RemovableDrivesInSidebar);
            OpenFileExplorerToThisPC = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.OpenFileExplorerToThisPC);
            RecentDocsHistory = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.RecentDocsHistory);
            RecentlyAddedApps = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.RecentlyAddedApps);
            ShortcutText = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.ShortcutText);
            SnapLayouts = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.SnapLayouts);
            StatusBar = await _registryService.IsSettingEnabled(RegistryKeys.FileExplorer.StatusBar);
            
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
        SystemFiles = _isSelectAll;
        ClassicContextMenu = _isSelectAll;
        AutoHideTaskbar = _isSelectAll;
        ThemeContextMenu = _isSelectAll;
        MouseAcceleration = _isSelectAll;
        MicrosoftCopilot = _isSelectAll;
        BackgroundApps = _isSelectAll;
        DevMode = _isSelectAll;
        Hibernation = _isSelectAll;
        Telemetry = _isSelectAll;
        CopyMoreDetails = _isSelectAll;
        DriveLetters = _isSelectAll;
        FoldersGroupToThisPC = _isSelectAll;
        Gallery = _isSelectAll;
        ControlPanelInNavigation = _isSelectAll;
        RecycleBinInNavigation = _isSelectAll;
        UserFolderInNavigation = _isSelectAll;
        RemovableDrivesInSidebar = _isSelectAll;
        OpenFileExplorerToThisPC = _isSelectAll;
        RecentDocsHistory = _isSelectAll;
        RecentlyAddedApps = _isSelectAll;
        ShortcutText = _isSelectAll;
        SnapLayouts = _isSelectAll;
        StatusBar = _isSelectAll;
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

            foreach (var setting in _modifiedSettings.ToList())
            {
                if (setting.Key == nameof(SearchboxTaskbarMode))
                {
                    await _registryService.SetValue(RegistryKeys.TaskBar.SearchboxTaskbarMode, SearchboxTaskbarMode);
                    _modifiedSettings.Remove(setting.Key);
                    continue;
                }

                var registryKey = GetRegistryKeyForSetting(setting.Key);
                await _registryService.SetSetting(registryKey, setting.Value);
                _modifiedSettings.Remove(setting.Key);
            }

            HasModifications = _modifiedSettings.Count > 0;
            
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
            nameof(SystemFiles) => RegistryKeys.FileExplorer.SystemFiles,
            nameof(ClassicContextMenu) => RegistryKeys.ContextMenu.ClassicContextMenu,
            nameof(AutoHideTaskbar) => RegistryKeys.ContextMenu.AutoHideTaskbar,
            nameof(ThemeContextMenu) => RegistryKeys.ContextMenu.ThemeContextMenu,
            nameof(MouseAcceleration) => RegistryKeys.SystemAndPerformance.MouseAcceleration,
            nameof(MicrosoftCopilot) => RegistryKeys.SystemAndPerformance.MicrosoftCopilot,
            nameof(BackgroundApps) => RegistryKeys.SystemAndPerformance.BackgroundApps,
            nameof(DevMode) => RegistryKeys.SystemAndPerformance.DevMode,
            nameof(Hibernation) => RegistryKeys.SystemAndPerformance.Hibernation,
            nameof(Telemetry) => RegistryKeys.SystemAndPerformance.Telemetry,
            nameof(CopyMoreDetails) => RegistryKeys.FileExplorer.CopyMoreDetails,
            nameof(DriveLetters) => RegistryKeys.FileExplorer.DriveLetters,
            nameof(FoldersGroupToThisPC) => RegistryKeys.FileExplorer.FoldersGroupToThisPC,
            nameof(Gallery) => RegistryKeys.FileExplorer.Gallery,
            nameof(ControlPanelInNavigation) => RegistryKeys.FileExplorer.ControlPanelInNavigation,
            nameof(RecycleBinInNavigation) => RegistryKeys.FileExplorer.RecycleBinInNavigation,
            nameof(UserFolderInNavigation) => RegistryKeys.FileExplorer.UserFolderInNavigation,
            nameof(RemovableDrivesInSidebar) => RegistryKeys.FileExplorer.RemovableDrivesInSidebar,
            nameof(OpenFileExplorerToThisPC) => RegistryKeys.FileExplorer.OpenFileExplorerToThisPC,
            nameof(RecentDocsHistory) => RegistryKeys.FileExplorer.RecentDocsHistory,
            nameof(RecentlyAddedApps) => RegistryKeys.FileExplorer.RecentlyAddedApps,
            nameof(ShortcutText) => RegistryKeys.FileExplorer.ShortcutText,
            nameof(SnapLayouts) => RegistryKeys.FileExplorer.SnapLayouts,
            nameof(StatusBar) => RegistryKeys.FileExplorer.StatusBar,
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

    [RelayCommand]
    private void ExpandAll()
    {
        IsTaskbarExpanded = true;
        IsPrivacyExpanded = true;
        IsFileExplorerExpanded = true;
        IsContextMenuExpanded = true;
        IsSystemExpanded = true;
    }

    [RelayCommand]
    private void CollapseAll()
    {
        IsTaskbarExpanded = false;
        IsPrivacyExpanded = false;
        IsFileExplorerExpanded = false;
        IsContextMenuExpanded = false;
        IsSystemExpanded = false;
    }
} 