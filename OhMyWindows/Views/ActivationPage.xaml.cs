using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class ActivationPage : Page
{
    public ActivationViewModel ViewModel
    {
        get;
    }

    public ActivationPage()
    {
        ViewModel = App.GetService<ActivationViewModel>();
        InitializeComponent();
    }
} 