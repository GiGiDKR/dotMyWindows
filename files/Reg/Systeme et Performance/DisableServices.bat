@echo off
:: Script pour modifier le statut de démarrage des services Windows non-essentiels
:: Source: https://christitustech.github.io/winutil/dev/tweaks/Essential-Tweaks/Services/

echo Modification des services en cours...

:: Connected User Experiences and Telemetry (DiagTrack) - Télémétrie Windows
sc config DiagTrack start= disabled
sc stop DiagTrack

:: dmwappushservice - WAP Push Message Routing Service
sc config dmwappushservice start= disabled
sc stop dmwappushservice

:: Downloaded Maps Manager (MapsBroker)
sc config MapsBroker start= disabled
sc stop MapsBroker

:: Xbox Live Services
sc config XboxGipSvc start= disabled
sc stop XboxGipSvc
sc config XblAuthManager start= disabled
sc stop XblAuthManager
sc config XblGameSave start= disabled
sc stop XblGameSave
sc config XboxNetApiSvc start= disabled
sc stop XboxNetApiSvc

:: Windows Mobile Hotspot Service
sc config icssvc start= disabled
sc stop icssvc

:: Windows Media Player Network Sharing
sc config WMPNetworkSvc start= disabled
sc stop WMPNetworkSvc

:: Windows Search
sc config WSearch start= disabled
sc stop WSearch

:: Remote Registry
sc config RemoteRegistry start= disabled
sc stop RemoteRegistry

:: Retail Demo Service
sc config RetailDemo start= disabled
sc stop RetailDemo

echo Terminé! Les services ont été configurés avec succès.
pause
