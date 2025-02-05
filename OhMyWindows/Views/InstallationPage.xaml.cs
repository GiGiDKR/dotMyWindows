using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class InstallationPage : Page
{
    public InstallationViewModel ViewModel { get; }

    public InstallationPage()
    {
        ViewModel = App.GetService<InstallationViewModel>();
        InitializeComponent();
    }
}
