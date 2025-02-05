# Vérifier si WinGet est déjà installé
if (Get-Command winget -ErrorAction SilentlyContinue) {
    Write-Host "WinGet est déjà installé."
    exit 0
}

# Télécharger et installer les dépendances de WinGet
$vcLibsUri = "https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx"
$vcLibsPath = Join-Path $env:TEMP "Microsoft.VCLibs.x64.14.00.Desktop.appx"
Invoke-WebRequest -Uri $vcLibsUri -OutFile $vcLibsPath
Add-AppxPackage $vcLibsPath

# Télécharger et installer WinGet
$latestWingetMsixBundleUri = "https://github.com/microsoft/winget-cli/releases/latest/download/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle"
$latestWingetMsixBundle = Join-Path $env:TEMP "Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle"
Invoke-WebRequest -Uri $latestWingetMsixBundleUri -OutFile $latestWingetMsixBundle
Add-AppxPackage $latestWingetMsixBundle

# Vérifier l'installation
if (Get-Command winget -ErrorAction SilentlyContinue) {
    Write-Host "WinGet a été installé avec succès."
    exit 0
} else {
    Write-Error "Erreur lors de l'installation de WinGet."
    exit 1
} 