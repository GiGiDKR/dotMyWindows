using Microsoft.UI.Xaml.Controls;
using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class HelpPage : Page
{
    public HelpViewModel ViewModel { get; }

    public HelpPage()
    {
        ViewModel = App.GetService<HelpViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
} 