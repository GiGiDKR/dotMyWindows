using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using OhMyWindows.Activation;
using OhMyWindows.Contracts.Services;
using OhMyWindows.Core.Contracts.Services;
using OhMyWindows.Core.Services;
using OhMyWindows.Helpers;
using OhMyWindows.Models;
using OhMyWindows.Notifications;
using OhMyWindows.Services;
using OhMyWindows.ViewModels;
using OhMyWindows.Views;

namespace OhMyWindows;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

#if !DEBUG
        if (!IsRunAsAdministrator())
        {
            RestartAsAdmin();
            Environment.Exit(0);
            return;
        }
#endif

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();
            services.AddSingleton<RegistryService>();
            services.AddSingleton<ProgramService>();
            services.AddSingleton<InstallationService>();
            services.AddSingleton<IInstalledPackagesService, InstalledPackagesService>();
            services.AddSingleton<PackageManagerInstallationService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<OutilsViewModel>();
            services.AddTransient<OutilsPage>();
            services.AddTransient<InstallationViewModel>();
            services.AddTransient<InstallationPage>();
            services.AddTransient<UninstallViewModel>();
            services.AddTransient<UninstallPage>();
            services.AddTransient<InstallPackagesViewModel>();
            services.AddTransient<InstallPackagesPage>();
            services.AddTransient<SystemeViewModel>();
            services.AddTransient<SystemePage>();
            services.AddTransient<LogicielsViewModel>();
            services.AddTransient<LogicielsPage>();
            services.AddTransient<TerminalViewModel>();
            services.AddTransient<TerminalPage>();
            services.AddTransient<ThemeViewModel>();
            services.AddTransient<ThemePage>();
            services.AddTransient<ActivationViewModel>();
            services.AddTransient<ActivationPage>();
            services.AddTransient<AndroidViewModel>();
            services.AddTransient<AndroidPage>();
            services.AddTransient<ConfigurationViewModel>();
            services.AddTransient<ConfigurationPage>();
            services.AddTransient<AcceuilViewModel>();
            services.AddTransient<AcceuilPage>();
            services.AddTransient<AboutViewModel>();
            services.AddTransient<AboutPage>();
            services.AddTransient<HelpViewModel>();
            services.AddTransient<HelpPage>();
            services.AddTransient<ShellPage>(sp => new ShellPage(sp.GetRequiredService<ShellViewModel>(), sp.GetRequiredService<IThemeSelectorService>()));
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    private bool IsRunAsAdministrator()
    {
        var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    private void RestartAsAdmin()
    {
        var processInfo = new System.Diagnostics.ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = Environment.ProcessPath,
            WorkingDirectory = Environment.CurrentDirectory,
            Verb = "runas"
        };

        try
        {
            System.Diagnostics.Process.Start(processInfo);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Privilèges administrateur requis",
                Content = "Cette application nécessite des privilèges administrateur pour fonctionner correctement. Veuillez la relancer en tant qu'administrateur.",
                CloseButtonText = "OK"
            };

            if (MainWindow?.Content != null)
            {
                dialog.XamlRoot = MainWindow.Content.XamlRoot;
                _ = dialog.ShowAsync();
            }
        }
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            base.OnLaunched(args);
            var activationService = App.GetService<IActivationService>();
            await activationService.ActivateAsync(args);
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "OhMyWindows", "startup_crash.log");
            var logDir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                try
                {
                    Directory.CreateDirectory(logDir);
                }
                catch (Exception dirEx)
                {
                    File.WriteAllText(logPath, $"{DateTime.Now:o} - Directory Creation Error: {dirEx}");
                    Application.Current.Exit();
                    return;
                }
            }
            File.WriteAllText(logPath, $"{DateTime.Now:o} - {ex}");
            Application.Current.Exit();
        }
    }
}
