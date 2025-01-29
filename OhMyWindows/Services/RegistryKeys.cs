using Microsoft.Win32;

namespace OhMyWindows.Services;

public static class RegistryKeys
{
    public static class TaskBar
    {
        private const string BasePath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        
        public static readonly RegistryKey TaskbarAlignment = new(
            BasePath,
            "TaskbarAl",
            "1", // Center
            "0"  // Left
        );

        public static readonly RegistryKey TaskbarEndTask = new(
            BasePath,
            "TaskbarEndTask",
            "1", // Enable
            "0"  // Disable
        );

        public static readonly RegistryKey TaskViewButton = new(
            BasePath,
            "ShowTaskViewButton",
            "1", // Show
            "0"  // Hide
        );
    }

    public static class Privacy
    {
        private const string BasePath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion";

        public static readonly RegistryKey BingSearch = new(
            BasePath + @"\Search",
            "BingSearchEnabled",
            "1", // Enable
            "0"  // Disable
        );

        public static readonly RegistryKey WebSearch = new(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer",
            "DisableSearchBoxSuggestions",
            "0", // Enable
            "1"  // Disable
        );

        public static readonly RegistryKey SubscribedContent = new(
            BasePath + @"\ContentDeliveryManager",
            "SubscribedContent-338393Enabled",
            "0", // Disable
            "1"  // Enable
        );

        public static readonly RegistryKey OfflineMaps = new(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Maps",
            "AutoDownloadAndUpdateMapData",
            "1", // Enable
            "0"  // Disable
        );
    }

    public static class FileExplorer
    {
        private const string BasePath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";

        public static readonly RegistryKey AppIconsThumbnails = new(
            BasePath,
            "ShowAppIconsOnThumbnails",
            "1", // Enable
            "0"  // Disable
        );

        public static readonly RegistryKey AutomaticFolderDiscovery = new(
            BasePath + @"\Advanced",
            "AutomaticFolderDiscovery",
            "1", // Enable
            "0"  // Disable
        );

        public static readonly RegistryKey CompactView = new(
            BasePath + @"\Advanced",
            "UseCompactMode",
            "1", // Enable
            "0"  // Disable
        );

        public static readonly RegistryKey FileExtension = new(
            BasePath + @"\Advanced",
            "HideFileExt",
            "0", // Show
            "1"  // Hide
        );

        public static readonly RegistryKey HiddenFiles = new(
            BasePath + @"\Advanced",
            "Hidden",
            "1", // Show
            "0"  // Hide
        );
    }

    public static class ContextMenu
    {
        public static readonly RegistryKey ClassicContextMenu = new(
            @"HKEY_CURRENT_USER\SOFTWARE\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32",
            "",
            "", // Enable (empty value)
            null // Disable (delete value)
        );

        public static readonly RegistryKey AutoHideTaskbar = new(
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "AutoHideTaskbar",
            "1", // Enable
            "0"  // Disable
        );

        public static readonly RegistryKey ThemeContextMenu = new(
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "TaskbarThemeSettings",
            "1", // Enable
            "0"  // Disable
        );
    }

    public static class SystemAndPerformance
    {
        private const string BasePath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion";

        public static readonly RegistryKey MouseAcceleration = new(
            BasePath + @"\MouseSettings",
            "MouseAcceleration",
            "0", // Disable
            "1"  // Enable
        );

        public static readonly RegistryKey MicrosoftCopilot = new(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot",
            "TurnOffWindowsCopilot",
            "1", // Disable
            "0"  // Enable
        );

        public static readonly RegistryKey BackgroundApps = new(
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications",
            "GlobalUserDisabled",
            "1", // Disable
            "0"  // Enable
        );

        public static readonly RegistryKey DevMode = new(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock",
            "AllowDevelopmentWithoutDevLicense",
            "1", // Enable
            "0"  // Disable
        );

        public static readonly RegistryKey Hibernation = new(
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power",
            "HibernateEnabled",
            "1", // Enable
            "0"  // Disable
        );

        public static readonly RegistryKey Telemetry = new(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
            "AllowTelemetry",
            "0", // Disable
            "1"  // Enable
        );
    }

    public class RegistryKey
    {
        public string Path { get; }
        public string Name { get; }
        public string? EnableValue { get; }
        public string? DisableValue { get; }

        public RegistryKey(string path, string name, string? enableValue, string? disableValue)
        {
            Path = path;
            Name = name;
            EnableValue = enableValue;
            DisableValue = disableValue;
        }
    }
} 