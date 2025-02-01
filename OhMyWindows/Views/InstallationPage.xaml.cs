using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OhMyWindows.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace OhMyWindows.Views;

public sealed partial class InstallationPage : Page
{
    public InstallationViewModel ViewModel
    {
        get;
    }

    public InstallationPage()
    {
        ViewModel = App.GetService<InstallationViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (ViewModel.InitializeCommand is IAsyncRelayCommand asyncCommand)
        {
            _ = asyncCommand.ExecuteAsync(null);
        }
        else
        {
            ViewModel.InitializeCommand.Execute(null);
        }
    }
}
