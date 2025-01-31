[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
[Windows.UI.Notifications.ToastNotification, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null

$template = @"
<toast>
    <visual>
        <binding template="ToastGeneric">
            <text>Notification</text>
            <text>Veuillez confirmer ou annuler l'action.</text>
        </binding>
    </visual>
    <actions>
        <action activationType="system" arguments="accept" content="Valider"/>
        <action activationType="system" arguments="cancel" content="Annuler"/>
    </actions>
</toast>
"@

$xml = New-Object Windows.Data.Xml.Dom.XmlDocument
$xml.LoadXml($template)

$toast = New-Object Windows.UI.Notifications.ToastNotification($xml)
$toast.ExpirationTime = [DateTimeOffset]::Now.AddMinutes(1)

[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier("Windows Notification").Show($toast)
