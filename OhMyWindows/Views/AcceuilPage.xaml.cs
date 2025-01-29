using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class AcceuilPage : Page
{
    public AcceuilViewModel ViewModel
    {
        get;
    }

    public AcceuilPage()
    {
        ViewModel = App.GetService<AcceuilViewModel>();
        InitializeComponent();
    }
}
