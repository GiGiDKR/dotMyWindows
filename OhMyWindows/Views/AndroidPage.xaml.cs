using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class AndroidPage : Page
{
    public AndroidViewModel ViewModel
    {
        get;
    }

    public AndroidPage()
    {
        ViewModel = App.GetService<AndroidViewModel>();
        InitializeComponent();
    }
} 