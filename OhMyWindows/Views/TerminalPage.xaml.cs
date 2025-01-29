using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class TerminalPage : Page
{
    public TerminalViewModel ViewModel
    {
        get;
    }

    public TerminalPage()
    {
        ViewModel = App.GetService<TerminalViewModel>();
        InitializeComponent();
    }
} 