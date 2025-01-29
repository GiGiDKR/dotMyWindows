using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class GestionnairePaquetsPage : Page
{
    public GestionnairePaquetsViewModel ViewModel
    {
        get;
    }

    public GestionnairePaquetsPage()
    {
        ViewModel = App.GetService<GestionnairePaquetsViewModel>();
        InitializeComponent();
    }
} 