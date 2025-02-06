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

    private string _selectAllButtonText = "Tout sélectionner";
    public string SelectAllButtonText
    {
        get => _selectAllButtonText;
        set => SetProperty(ref _selectAllButtonText, value);
    }

    private bool _canExpandAll = true;
    public bool CanExpandAll
    {
        get => _canExpandAll;
        set => SetProperty(ref _canExpandAll, value);
    }

    private bool _canCollapseAll = false;
    public bool CanCollapseAll
    {
        get => _canCollapseAll;
        set => SetProperty(ref _canCollapseAll, value);
    }

    private bool _isTaskbarExpanded;
    public bool IsTaskbarExpanded
    {
        get => _isTaskbarExpanded;
        set => SetProperty(ref _isTaskbarExpanded, value);
    }

    private bool _isPrivacyExpanded;
    public bool IsPrivacyExpanded
    {
        get => _isPrivacyExpanded;
        set => SetProperty(ref _isPrivacyExpanded, value);
    }

    private bool _isFileExplorerExpanded;
    public bool IsFileExplorerExpanded
    {
        get => _isFileExplorerExpanded;
        set => SetProperty(ref _isFileExplorerExpanded, value);
    }

    private bool _isContextMenuExpanded;
    public bool IsContextMenuExpanded
    {
        get => _isContextMenuExpanded;
        set => SetProperty(ref _isContextMenuExpanded, value);
    }

    private bool _isSystemExpanded;
    public bool IsSystemExpanded
    {
        get => _isSystemExpanded;
        set => SetProperty(ref _isSystemExpanded, value);
    }

    private bool _taskbarAlignmentCenter;
    public bool TaskbarAlignmentCenter
    {
        get => _taskbarAlignmentCenter;
        set => SetProperty(ref _taskbarAlignmentCenter, value);
    }

    private bool _taskbarEndTask;
    public bool TaskbarEndTask
    {
        get => _taskbarEndTask;
        set => SetProperty(ref _taskbarEndTask, value);
    }

    private bool _taskViewButton;
    public bool TaskViewButton
    {
        get => _taskViewButton;
        set => SetProperty(ref _taskViewButton, value);
    }

    private bool _bingSearch;
    public bool BingSearch
    {
        get => _bingSearch;
        set => SetProperty(ref _bingSearch, value);
    }

    private bool _webSearch;
    public bool WebSearch
    {
        get => _webSearch;
        set => SetProperty(ref _webSearch, value);
    }

    private bool _subscribedContent;
    public bool SubscribedContent
    {
        get => _subscribedContent;
        set => SetProperty(ref _subscribedContent, value);
    }

    private bool _offlineMaps;
    public bool OfflineMaps
    {
        get => _offlineMaps;
        set => SetProperty(ref _offlineMaps, value);
    }

    private bool _appIconsThumbnails;
    public bool AppIconsThumbnails
    {
        get => _appIconsThumbnails;
        set => SetProperty(ref _appIconsThumbnails, value);
    }

    private bool _automaticFolderDiscovery;
    public bool AutomaticFolderDiscovery
    {
        get => _automaticFolderDiscovery;
        set => SetProperty(ref _automaticFolderDiscovery, value);
    }

    private bool _compactView;
    public bool CompactView
    {
        get => _compactView;
        set => SetProperty(ref _compactView, value);
    }

    private bool _fileExtension;
    public bool FileExtension
    {
        get => _fileExtension;
        set => SetProperty(ref _fileExtension, value);
    }

    private bool _hiddenFiles;
    public bool HiddenFiles
    {
        get => _hiddenFiles;
        set => SetProperty(ref _hiddenFiles, value);
    }

    private bool _classicContextMenu;
    public bool ClassicContextMenu
    {
        get => _classicContextMenu;
        set => SetProperty(ref _classicContextMenu, value);
    }

    private bool _systemFiles;
    public bool SystemFiles
    {
        get => _systemFiles;
        set => SetProperty(ref _systemFiles, value);
    }

    private bool _autoHideTaskbar;
    public bool AutoHideTaskbar
    {
        get => _autoHideTaskbar;
        set => SetProperty(ref _autoHideTaskbar, value);
    }

    private bool _themeContextMenu;
    public bool ThemeContextMenu
    {
        get => _themeContextMenu;
        set => SetProperty(ref _themeContextMenu, value);
    }

    private bool _mouseAcceleration;
    public bool MouseAcceleration
    {
        get => _mouseAcceleration;
        set => SetProperty(ref _mouseAcceleration, value);
    }

    private bool _microsoftCopilot;
    public bool MicrosoftCopilot
    {
        get => _microsoftCopilot;
        set => SetProperty(ref _microsoftCopilot, value);
    }

    private bool _backgroundApps;
    public bool BackgroundApps
    {
        get => _backgroundApps;
        set => SetProperty(ref _backgroundApps, value);
    }

    private bool _devMode;
    public bool DevMode
    {
        get => _devMode;
        set => SetProperty(ref _devMode, value);
    }

    private bool _hibernation;
    public bool Hibernation
    {
        get => _hibernation;
        set => SetProperty(ref _hibernation, value);
    }

    private bool _telemetry;
    public bool Telemetry
    {
        get => _telemetry;
        set => SetProperty(ref _telemetry, value);
    }

    private bool _hasModifications;

    private int _searchboxTaskbarMode = 1;
    public int SearchboxTaskbarMode
    {
        get => _searchboxTaskbarMode;
        set => SetProperty(ref _searchboxTaskbarMode, value);
    }
    public bool HasModifications
    {
        get => _hasModifications;
        set => SetProperty(ref _hasModifications, value);
    }

    private bool _copyMoreDetails;
    public bool CopyMoreDetails
    {
        get => _copyMoreDetails;
        set => SetProperty(ref _copyMoreDetails, value);
    }

    private bool _driveLetters;
    public bool DriveLetters
    {
        get => _driveLetters;
        set => SetProperty(ref _driveLetters, value);
    }

    private bool _foldersGroupToThisPC;
    public bool FoldersGroupToThisPC
    {
        get => _foldersGroupToThisPC;
        set => SetProperty(ref _foldersGroupToThisPC, value);
    }

    private bool _gallery;
    public bool Gallery
    {
        get => _gallery;
        set => SetProperty(ref _gallery, value);
    }

    private bool _controlPanelInNavigation;
    public bool ControlPanelInNavigation
    {
        get => _controlPanelInNavigation;
        set => SetProperty(ref _controlPanelInNavigation, value);
    }

    private bool _recycleBinInNavigation;
    public bool RecycleBinInNavigation
    {
        get => _recycleBinInNavigation;
        set => SetProperty(ref _recycleBinInNavigation, value);
    }

    private bool _userFolderInNavigation;
    public bool UserFolderInNavigation
    {
        get => _userFolderInNavigation;
        set => SetProperty(ref _userFolderInNavigation, value);
    }

    private bool _removableDrivesInSidebar;
    public bool RemovableDrivesInSidebar
    {
        get => _removableDrivesInSidebar;
        set => SetProperty(ref _removableDrivesInSidebar, value);
    }

    private bool _openFileExplorerToThisPC;
    public bool OpenFileExplorerToThisPC
    {
        get => _openFileExplorerToThisPC;
        set => SetProperty(ref _openFileExplorerToThisPC, value);
    }

    private bool _recentDocsHistory;
    public bool RecentDocsHistory
    {
        get => _recentDocsHistory;
        set => SetProperty(ref _recentDocsHistory, value);
    }

    private bool _recentlyAddedApps;
    public bool RecentlyAddedApps
    {
        get => _recentlyAddedApps;
        set => SetProperty(ref _recentlyAddedApps, value);
    }

    private bool _shortcutText;
    public bool ShortcutText
    {
        get => _shortcutText;
        set => SetProperty(ref _shortcutText, value);
    }

    private bool _snapLayouts;
    public bool SnapLayouts
    {
        get => _snapLayouts;
        set => SetProperty(ref _snapLayouts, value);
    }

    private bool _statusBar;
    public bool StatusBar
    {
        get => _statusBar;
        set => SetProperty(ref _statusBar, value);
    }

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