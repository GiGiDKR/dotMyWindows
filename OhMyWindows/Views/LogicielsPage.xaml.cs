using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class LogicielsPage : Page
{
    public LogicielsViewModel ViewModel
    {
        get;
    }

    public LogicielsPage()
    {
        ViewModel = App.GetService<LogicielsViewModel>();
        InitializeComponent();
    }
} 