using Microsoft.UI.Xaml.Controls;
using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class DesinstallationPage : Page
{
    public DesinstallationViewModel ViewModel
    {
        get;
    }

    public DesinstallationPage()
    {
        ViewModel = App.GetService<DesinstallationViewModel>();
        InitializeComponent();
    }
}
