using Microsoft.UI.Xaml.Controls;
using OhMyWindows.ViewModels;

namespace OhMyWindows.Views;

public sealed partial class SystemePage : Page
{
    public SystemeViewModel ViewModel { get; }

    public SystemePage()
    {
        ViewModel = App.GetService<SystemeViewModel>();
        InitializeComponent();

        // Les liaisons de données sont maintenant gérées directement dans le XAML
        // et les modifications sont suivies dans le ViewModel
    }
} 