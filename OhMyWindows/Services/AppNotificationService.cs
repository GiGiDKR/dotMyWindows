using System.Collections.Specialized;
using System.Web;

using Microsoft.Windows.AppNotifications;

using OhMyWindows.Contracts.Services;
using OhMyWindows.ViewModels;

namespace OhMyWindows.Notifications;

public class AppNotificationService : IAppNotificationService
{
    private readonly INavigationService _navigationService;

    public AppNotificationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

        AppNotificationManager.Default.Register();

        // Afficher une notification toast au démarrage avec un format amélioré
        ShowWelcomeNotification();
    }

    private void ShowWelcomeNotification()
    {
        var toastXml = $@"<toast duration='long'>
            <visual>
                <binding template='ToastGeneric'>
                    <text>Bienvenue sur OhMyWindows</text>
                    <text>Votre assistant de gestion des applications Windows</text>
                </binding>
            </visual>
            <actions>
                <action content='Ouvrir' arguments='action=open'/>
                <action content='Fermer' arguments='action=dismiss'/>
            </actions>
        </toast>";

        Show(toastXml);
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        var arguments = ParseArguments(args.Argument);
        var action = arguments["action"];

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            if (action == "open")
            {
                App.MainWindow.BringToFront();
            }
            else if (action == "dismiss")
            {
                App.MainWindow.Close();
            }
        });
    }

    public bool Show(string payload)
    {
        var appNotification = new AppNotification(payload);

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
