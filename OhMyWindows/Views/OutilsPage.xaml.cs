using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class OutilsPage : Page
{
    public OutilsViewModel ViewModel
    {
        get;
    }

    public OutilsPage()
    {
        ViewModel = App.GetService<OutilsViewModel>();
        InitializeComponent();
    }
}
