using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class ProgrammesPage : Page
{
    public ProgrammesViewModel ViewModel
    {
        get;
    }

    public ProgrammesPage()
    {
        ViewModel = App.GetService<ProgrammesViewModel>();
        InitializeComponent();
    }
} 