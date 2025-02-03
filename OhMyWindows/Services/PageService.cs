using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;

using OhMyWindows.Contracts.Services;
using OhMyWindows.ViewModels;
using OhMyWindows.Views;

namespace OhMyWindows.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<AcceuilViewModel, AcceuilPage>();
        Configure<ConfigurationViewModel, ConfigurationPage>();
        Configure<SystemeViewModel, SystemePage>();
        Configure<LogicielsViewModel, LogicielsPage>();
        Configure<TerminalViewModel, TerminalPage>();
        Configure<ThemeViewModel, ThemePage>();
        Configure<InstallationViewModel, InstallationPage>();
        Configure<ProgrammesViewModel, ProgrammesPage>();
        Configure<GestionnairePaquetsViewModel, GestionnairePaquetsPage>();
        Configure<OutilsViewModel, OutilsPage>();
        Configure<ActivationViewModel, ActivationPage>();
        Configure<AndroidViewModel, AndroidPage>();
        Configure<DesinstallationViewModel, DesinstallationPage>();
        Configure<SettingsViewModel, SettingsPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
