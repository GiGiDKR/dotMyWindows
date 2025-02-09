using Microsoft.UI.Xaml.Controls;
using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel { get; }

    public AboutPage()
    {
        ViewModel = App.GetService<AboutViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
} 