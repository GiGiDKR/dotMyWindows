using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class ThemePage : Page
{
    public ThemeViewModel ViewModel
    {
        get;
    }

    public ThemePage()
    {
        ViewModel = App.GetService<ThemeViewModel>();
        InitializeComponent();
    }
} 