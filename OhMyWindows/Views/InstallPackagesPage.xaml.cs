using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OhMyWindows.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace OhMyWindows.Views;

public sealed partial class InstallPackagesPage : Page

{
    public InstallPackagesViewModel ViewModel
    {
        get;
    }


    public InstallPackagesPage()
    {
        try
        {
            ViewModel = App.GetService<InstallPackagesViewModel>();
            InitializeComponent();
        }
        catch (Exception ex)
        {
            // En cas d'erreur, afficher un message à l'utilisateur
            var dialog = new ContentDialog
            {
                Title = "Erreur",
                Content = $"Une erreur s'est produite lors de l'initialisation : {ex.Message}",
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        try
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
        catch (Exception ex)
        {
            // En cas d'erreur, afficher un message à l'utilisateur
            var dialog = new ContentDialog
            {
                Title = "Erreur",
                Content = $"Une erreur s'est produite lors du chargement : {ex.Message}",
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
        }
    }
}
