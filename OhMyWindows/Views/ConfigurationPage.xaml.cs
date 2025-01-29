using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class ConfigurationPage : Page
{
    public ConfigurationViewModel ViewModel
    {
        get;
    }

    public ConfigurationPage()
    {
        ViewModel = App.GetService<ConfigurationViewModel>();
        InitializeComponent();
    }
}
