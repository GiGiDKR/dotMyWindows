using Microsoft.UI.Xaml.Controls;

using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class SystemePage : Page
{
    public SystemeViewModel ViewModel
    {
        get;
    }

    public SystemePage()
    {
        ViewModel = App.GetService<SystemeViewModel>();
        InitializeComponent();
    }
} 