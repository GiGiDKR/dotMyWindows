using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.Messaging;
using OhMyWindows.Messages;
using OhMyWindows.ViewModels;
using System.ComponentModel;
using OhMyWindows.Models;

namespace OhMyWindows.Views;

public sealed partial class UninstallPage : Page, IRecipient<ShowUninstallConfirmationMessage>, IDisposable
{
    private bool _disposed;
    private readonly IMessenger _messenger;


    public UninstallViewModel ViewModel { get; }

    public UninstallPage()

    {
        ViewModel = App.GetService<UninstallViewModel>() ?? throw new InvalidOperationException("Failed to resolve ViewModel");
        _messenger = WeakReferenceMessenger.Default;
        InitializeComponent();
        _messenger.Register(this);
    }


    private void ProgramsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView listView)
        {
            ViewModel.SelectedPrograms = listView.SelectedItems.Cast<InstalledProgram>().ToList();
        }
    }

    public async void Receive(ShowUninstallConfirmationMessage message)
    {
        if (_disposed || UninstallConfirmationDialog == null) return;

        try
        {
            var result = await UninstallConfirmationDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !_disposed && ViewModel?.ConfirmUninstallCommand != null)
            {
                await ViewModel.ConfirmUninstallCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            // Log ou gérer l'erreur selon les besoins
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la désinstallation : {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _messenger.Unregister<ShowUninstallConfirmationMessage>(this);
            _disposed = true;
        }
    }

    ~UninstallPage()
    {
        Dispose();
    }

}
