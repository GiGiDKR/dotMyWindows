using Microsoft.Win32;

namespace OhMyWindows.Services;

public static class RegistryKeys
{
    public static class TaskBar
    {
        private const string BasePath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        private const string DeveloperSettingsPath = BasePath + @"\TaskbarDeveloperSettings";
        
        public static readonly RegistryKey TaskbarAlignment = new(
            BasePath,
            "TaskbarAl",
            "1", // Center
            "0", // Left
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey SearchboxTaskbarMode = new(
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Search",
            "SearchboxTaskbarMode",
            null, // Pas de valeur enable/disable car c'est un mode
            null,
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey TaskbarEndTask = new(
            DeveloperSettingsPath,
            "TaskbarEndTask",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey TaskViewButton = new(
            BasePath,
            "ShowTaskViewButton",
            "1", // Show
            "0", // Hide
            RegistryValueKind.DWord
        );
    }

    public static class Privacy
    {
        private const string BasePath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion";
        private const string PoliciesPath = @"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows";

        public static readonly RegistryKey BingSearch = new(
            BasePath + @"\Search",
            "BingSearchEnabled",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey WebSearch = new(
            PoliciesPath + @"\Explorer",
            "DisableSearchBoxSuggestions",
            "0", // Enable
            "1", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey SubscribedContent = new(
            BasePath + @"\ContentDeliveryManager",
            "SubscribedContent-338388Enabled",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey OfflineMaps = new(
            PoliciesPath + @"\Maps",
            "AutoDownloadAndUpdateMapData",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );
    }

    public static class FileExplorer
    {
        private const string BasePath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
        private const string AdvancedPath = BasePath + @"\Advanced";

        public static readonly RegistryKey AppIconsThumbnails = new(
            BasePath + @"\Advanced",
            "ShowAppIconsOnThumbnails",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey AutomaticFolderDiscovery = new(
            BasePath + @"\Advanced",
            "AutomaticFolderDiscovery",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey CompactView = new(
            BasePath + @"\Advanced",
            "UseCompactMode",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey FileExtension = new(
            BasePath + @"\Advanced",
            "HideFileExt",
            "0", // Show
            "1", // Hide
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey HiddenFiles = new(
            BasePath + @"\Advanced",
            "Hidden",
            "1", // Show
            "0", // Hide
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey SystemFiles = new(
            BasePath + @"\Advanced",
            "ShowSuperHidden",
            "1", // Show
            "0", // Hide
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey CopyMoreDetails = new(
            AdvancedPath,
            "ShowInfoTip",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey DriveLetters = new(
            AdvancedPath,
            "ShowDriveLetters",
            "1", // Show
            "0", // Hide
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey FoldersGroupToThisPC = new(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FolderDescriptions",
            "ThisPCPolicy",
            "Show", // Show
            "Hide", // Hide
            RegistryValueKind.String
        );

        public static readonly RegistryKey Gallery = new(
            AdvancedPath,
            "ShowGallery",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey ControlPanelInNavigation = new(
            AdvancedPath,
            "ShowControlPanel",
            "1", // Show
            "0", // Hide
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey RecycleBinInNavigation = new(
            AdvancedPath,
            "ShowRecycleBin",
            "1", // Show
            "0", // Hide
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey UserFolderInNavigation = new(
            AdvancedPath,
            "ShowUserFolder",
            "1", // Show
            "0", // Hide
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey RemovableDrivesInSidebar = new(
            AdvancedPath,
            "ShowRemovableDrives",
            "1", // Show
            "0", // Hide
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey OpenFileExplorerToThisPC = new(
            AdvancedPath,
            "LaunchTo",
            "1", // This PC
            "2", // Home
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey RecentDocsHistory = new(
            BasePath + @"\Advanced",
            "Start_TrackDocs",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey RecentlyAddedApps = new(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer",
            "HideRecentlyAddedApps",
            "0", // Show
            "1", // Hide
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey ShortcutText = new(
            AdvancedPath,
            "Link",
            "0", // Disable
            "1", // Enable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey SnapLayouts = new(
            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "EnableSnapAssistFlyout",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey StatusBar = new(
            AdvancedPath,
            "ShowStatusBar",
            "1", // Show
            "0", // Hide
            RegistryValueKind.DWord
        );
    }

    public static class ContextMenu
    {
        private const string BasePath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

        public static readonly RegistryKey ClassicContextMenu = new(
            @"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32",
            "@",  // Nom de la valeur par défaut
            "", // Enable (empty value)
            null, // Disable (delete value)
            RegistryValueKind.String
        );

        public static readonly RegistryKey AutoHideTaskbar = new(
            BasePath,
            "TaskbarAutoHide",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey ThemeContextMenu = new(
            BasePath,
            "TaskbarThemeSettings",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );
    }

    public static class SystemAndPerformance
    {
        private const string BasePath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion";
        private const string PoliciesPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows";

        public static readonly RegistryKey MouseAcceleration = new(
            @"HKEY_CURRENT_USER\Control Panel\Mouse",
            "MouseSpeed",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.String
        );

        public static readonly RegistryKey MouseThreshold1 = new(
            @"HKEY_CURRENT_USER\Control Panel\Mouse",
            "MouseThreshold1",
            "6", // Default
            null,
            RegistryValueKind.String
        );

        public static readonly RegistryKey MouseThreshold2 = new(
            @"HKEY_CURRENT_USER\Control Panel\Mouse",
            "MouseThreshold2",
            "10", // Default
            null,
            RegistryValueKind.String
        );

        public static readonly RegistryKey MicrosoftCopilot = new(
            PoliciesPath + @"\WindowsCopilot",
            "TurnOffWindowsCopilot",
            "1", // Disable
            "0", // Enable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey BackgroundApps = new(
            BasePath + @"\BackgroundAccessApplications",
            "GlobalUserDisabled",
            "1", // Disable
            "0", // Enable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey BackgroundAppsSearch = new(
            BasePath + @"\Search",
            "BackgroundAppGlobalToggle",
            "0", // Disable
            "1", // Enable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey DevMode = new(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock",
            "AllowDevelopmentWithoutDevLicense",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey Hibernation = new(
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power",
            "HibernateEnabled",
            "1", // Enable
            "0", // Disable
            RegistryValueKind.DWord
        );

        public static readonly RegistryKey Telemetry = new(
            PoliciesPath + @"\DataCollection",
            "AllowTelemetry",
            "0", // Disable
            "1", // Enable
            RegistryValueKind.DWord
        );
    }

    public class RegistryKey
    {
        public string Path { get; }
        public string Name { get; }
        public string? EnableValue { get; }
        public string? DisableValue { get; }
        public RegistryValueKind ValueKind { get; }

        public RegistryKey(string path, string name, string? enableValue, string? disableValue, RegistryValueKind valueKind)
        {
            Path = path;
            Name = name;
            EnableValue = enableValue;
            DisableValue = disableValue;
            ValueKind = valueKind;
        }
    }
} 