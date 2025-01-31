# Chemin du fichier log
$logFile = Join-Path $PSScriptRoot "ConfigureServices.log"

# Fonction pour écrire dans le log
function Write-Log {
    param(
        [string]$Message,
        [string]$Type = "INFO"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp [$Type] $Message" | Out-File -FilePath $logFile -Append
}

# Fonction pour vérifier les droits administrateur
function Test-Administrator {
    $currentUser = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $currentUser.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Relancer le script avec élévation de droits si nécessaire
if (-not (Test-Administrator)) {
    try {
        Write-Host "Configuration des services Windows en cours..."
        Start-Process PowerShell -Verb RunAs -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -WindowStyle Normal
        exit
    }
    catch {
        Write-Log "Erreur lors de l'elevation des droits : $_" -Type "ERROR"
        exit 1
    }
}

# Initialiser le fichier de log
if (Test-Path $logFile) {
    Clear-Content $logFile
}

Write-Host "Configuration des services Windows en cours..."
Write-Log "Debut de la configuration des services Windows"

# Fonction pour configurer un service
function Set-ServiceConfig {
    param(
        [string]$ServiceName,
        [string]$StartupType,
        [string]$Description,
        [bool]$Delayed = $false
    )
    try {
        if ($Delayed -and $StartupType -eq "Automatic") {
            # Configuration du démarrage différé via le registre
            $regPath = "HKLM:\SYSTEM\CurrentControlSet\Services\$ServiceName"
            Set-ItemProperty -Path $regPath -Name "DelayedAutostart" -Value 1 -Type DWORD
            Write-Log "Service $ServiceName configure en demarrage automatique differe : $Description"
        }
        
        Set-Service -Name $ServiceName -StartupType $StartupType -ErrorAction Stop
        if ($StartupType -eq "Disabled") {
            Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
        }
        Write-Log "Service $ServiceName configure en $StartupType : $Description"
    }
    catch {
        Write-Log "Erreur lors de la configuration de $ServiceName : $_" -Type "ERROR"
    }
}

# Services d'infrastructure (Automatic Delayed)
$AutomaticDelayedServices = @{
    "TrustedInstaller" = "Windows Modules Installer"
    "gpsvc" = "Client de stratégie de groupe"
    "FontCache" = "Service de cache de police Windows"
    "BrokerInfrastructure" = "Service d'infrastructure des tâches en arrière-plan"
    "sppsvc" = "Protection logicielle"
}

# Services périphériques (Manual - Trigger Start)
$ManualTriggerServices = @{
    "Audiosrv" = "Audio Windows"
    "bthserv" = "Bluetooth Driver Management Service"
    "kdsSvc" = "Détection matériel noyau"
    "DeviceAssociationService" = "Service d'association de périphérique"
}

# Services à désactiver (Disabled)
$DisabledServices = @{
    "DiagTrack" = "Connected User Experiences and Telemetry"
    "dmwappushservice" = "WAP Push Message Routing Service"
    "MapsBroker" = "Downloaded Maps Manager"
    "XboxGipSvc" = "Xbox Accessory Management Service"
    "XblAuthManager" = "Xbox Live Auth Manager"
    "XblGameSave" = "Xbox Live Game Save"
    "XboxNetApiSvc" = "Xbox Live Networking Service"
    "icssvc" = "Windows Mobile Hotspot Service"
    "WMPNetworkSvc" = "Windows Media Player Network Sharing"
    "WSearch" = "Windows Search"
    "RemoteRegistry" = "Remote Registry"
    "RetailDemo" = "Retail Demo Service"
}

# Services à configurer en manuel (Manual)
$ManualServices = @{
    "GamingServicesNet" = "Gaming Services"
    "igccservice" = "Intel(R) Graphics Command Center Service"
    "TabletInputService" = "Service de gestion des entrées de texte"
    "CmService" = "Service du gestionnaire de conteneurs"
    "BITS" = "Background Intelligent Transfer Service"
    "DPS" = "Diagnostic Policy Service"
    "WdiServiceHost" = "Diagnostic Service Host"
    "WdiSystemHost" = "Diagnostic System Host"
    "TrkWks" = "Distributed Link Tracking Client"
    "EFS" = "Encrypting File System"
    "Fax" = "Fax Service"
    "lfsvc" = "Geolocation Service"
    "iphlpsvc" = "IP Helper"
    "MSiSCSI" = "Microsoft iSCSI Initiator Service"
    "NetTcpPortSharing" = ".NET TCP Port Sharing Service"
    "CscService" = "Offline Files"
    "PhoneSvc" = "Phone Service"
    "WpcMonSvc" = "Parental Controls"
    "SessionEnv" = "Remote Desktop Configuration"
    "TermService" = "Remote Desktop Services"
    "UmRdpService" = "Remote Desktop Services UserMode Port Redirector"
    "RpcLocator" = "Remote Procedure Call Locator"
    "RemoteAccess" = "Routing and Remote Access"
    "SCardSvr" = "Smart Card"
    "SCPolicySvc" = "Smart Card Removal Policy"
    "SNMPTRAP" = "SNMP Trap"
    "WalletService" = "Wallet Service"
    "wcncsvc" = "Windows Connect Now"
    "WerSvc" = "Windows Error Reporting Service"
    "wisvc" = "Windows Insider Service"
    "WwanSvc" = "WWAN AutoConfig"
}

# Services à configurer en automatique (Automatic)
$AutomaticServices = @{
    "AudioEndpointBuilder" = "Windows Audio Endpoint Builder"
    "Audiosrv" = "Windows Audio"
}

Write-Log "Configuration des services a demarrage automatique differe (delayed)..."
foreach ($service in $AutomaticDelayedServices.GetEnumerator()) {
    Set-ServiceConfig -ServiceName $service.Key -StartupType "Automatic" -Description $service.Value -Delayed $true
}

Write-Log "Configuration des services a demarrage manuel (trigger)..."
foreach ($service in $ManualTriggerServices.GetEnumerator()) {
    Set-ServiceConfig -ServiceName $service.Key -StartupType "Manual" -Description $service.Value
}

Write-Log "Configuration des services desactives..."
foreach ($service in $DisabledServices.GetEnumerator()) {
    Set-ServiceConfig -ServiceName $service.Key -StartupType "Disabled" -Description $service.Value
}

Write-Log "Configuration des services manuels..."
foreach ($service in $ManualServices.GetEnumerator()) {
    Set-ServiceConfig -ServiceName $service.Key -StartupType "Manual" -Description $service.Value
}

Write-Log "Configuration des services automatiques..."
foreach ($service in $AutomaticServices.GetEnumerator()) {
    Set-ServiceConfig -ServiceName $service.Key -StartupType "Automatic" -Description $service.Value
}

Write-Log "Configuration des services terminee !"
