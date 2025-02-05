[CmdletBinding()]
param (
    [switch]$Force,
    [switch]$ForceClose,
    [switch]$CheckForUpdate,
    [switch]$Wait,
    [switch]$NoExit,
    [switch]$UpdateSelf,
    [switch]$Version,
    [switch]$Help
)

# Préférences
$ProgressPreference = 'SilentlyContinue'
$ConfirmPreference = 'None'

# Définir les préférences de débogage si -Debug est spécifié
if ($PSBoundParameters.ContainsKey('Debug') -and $PSBoundParameters['Debug']) {
    $DebugPreference = 'Continue'
    $ConfirmPreference = 'None'
}

function Get-OSInfo {
    [CmdletBinding()]
    param ()

    try {
        # Obtenir les valeurs du registre
        $registryValues = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion"
        $releaseIdValue = $registryValues.ReleaseId
        $displayVersionValue = $registryValues.DisplayVersion
        $nameValue = $registryValues.ProductName
        $editionIdValue = $registryValues.EditionId

        # Supprimer "Server" de $editionIdValue s'il existe
        $editionIdValue = $editionIdValue -replace "Server", ""

        # Obtenir les détails du système d'exploitation
        $osDetails = Get-CimInstance -ClassName Win32_OperatingSystem
        $nameValue = $osDetails.Caption

        # Obtenir l'architecture
        $architecture = ($osDetails.OSArchitecture -replace "[^\d]").Trim()
        if ($architecture -eq "32") {
            $architecture = "x32"
        } elseif ($architecture -eq "64") {
            $architecture = "x64"
        }

        # Obtenir la version du système d'exploitation
        $versionValue = [System.Environment]::OSVersion.Version

        # Déterminer le type de produit
        if ($osDetails.ProductType -eq 1) {
            $typeValue = "Workstation"
        } elseif ($osDetails.ProductType -eq 2 -or $osDetails.ProductType -eq 3) {
            $typeValue = "Server"
        } else {
            $typeValue = "Unknown"
        }

        # Extraire la version numérique du nom
        $numericVersion = ($nameValue -replace "[^\d]").Trim()

        # Si la version est 10 ou supérieure et contient "multi-session", considérer comme workstation
        if ($numericVersion -ge 10 -and $osDetails.Caption -match "multi-session") {
            $typeValue = "Workstation"
        }

        return [PSCustomObject]@{
            ReleaseId      = $releaseIdValue
            DisplayVersion = $displayVersionValue
            Name           = $nameValue
            Type          = $typeValue
            NumericVersion = $numericVersion
            EditionId      = $editionIdValue
            Version       = $versionValue
            Architecture  = $architecture
        }
    } catch {
        Write-Error "Impossible d'obtenir les informations du système.`nErreur: $_"
        exit 1
    }
}

function Test-AdminPrivileges {
    if (([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        return $true
    }
    return $false
}

function Write-Section($text) {
    Write-Output ""
    Write-Output ("#" * ($text.Length + 4))
    Write-Output "# $text #"
    Write-Output ("#" * ($text.Length + 4))
    Write-Output ""
}

function Get-WingetStatus {
    $winget = Get-Command -Name winget -ErrorAction SilentlyContinue
    if ($null -ne $winget -and $winget -notlike '*failed to run*') {
        return $true
    }
    return $false
}

function Handle-Error {
    param($ErrorRecord)
    
    $OriginalErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = 'SilentlyContinue'

    if ($ErrorRecord.Exception.Message -match '0x80073D06') {
        Write-Warning "Une version plus récente est déjà installée."
        Write-Warning "Pas de problème, on continue..."
    } elseif ($ErrorRecord.Exception.Message -match '0x80073CF0') {
        Write-Warning "La même version est déjà installée."
        Write-Warning "Pas de problème, on continue..."
    } elseif ($ErrorRecord.Exception.Message -match '0x80073D02') {
        Write-Warning "Les ressources sont en cours d'utilisation. Essayez de fermer Windows Terminal / PowerShell / Command Prompt et réessayez."
        return $ErrorRecord
    } elseif ($ErrorRecord.Exception.Message -match '0x80073CF3') {
        Write-Warning "Problème avec l'une des dépendances."
        Write-Warning "Essayez de relancer le script."
        return $ErrorRecord
    } else {
        return $ErrorRecord
    }

    $ErrorActionPreference = $OriginalErrorActionPreference
}

function Add-ToEnvironmentPath {
    param (
        [Parameter(Mandatory = $true)]
        [string]$PathToAdd,
        [Parameter(Mandatory = $true)]
        [ValidateSet('User', 'System')]
        [string]$Scope
    )

    if ($Scope -eq 'System') {
        $envPath = [System.Environment]::GetEnvironmentVariable('PATH', [System.EnvironmentVariableTarget]::Machine)
        if ($envPath -notlike "*$PathToAdd*") {
            $envPath += ";$PathToAdd"
            [System.Environment]::SetEnvironmentVariable('PATH', $envPath, [System.EnvironmentVariableTarget]::Machine)
            Write-Debug "Ajout de $PathToAdd au PATH système."
        }
    }
    
    # Mettre à jour le PATH du processus actuel
    $env:PATH += ";$PathToAdd"
}

# Vérification initiale des privilèges administrateur
if (-not (Test-AdminPrivileges)) {
    Write-Error "L'installation de WinGet nécessite des privilèges administrateur."
    exit 1
}

# Obtenir la version du système d'exploitation
$osVersion = Get-OSInfo

# Vérifier si WinGet est déjà installé
if (Get-WingetStatus) {
    if ($Force -eq $false) {
        Write-Warning "WinGet est déjà installé."
        Write-Warning "Pour réinstaller WinGet, utilisez le paramètre -Force."
        exit 0
    }
}

try {
    # Installation de WinGet
    Write-Section "Installation de WinGet"

    try {
        Write-Output "Installation du module Microsoft.WinGet.Client..."
        Install-Module -Name Microsoft.WinGet.Client -Force -AllowClobber -Repository PSGallery -ErrorAction SilentlyContinue

        Write-Output "Installation de WinGet (cela peut prendre quelques minutes)..."
        Repair-WinGetPackageManager -AllUsers
    } catch {
        $errorHandled = Handle-Error $_
        if ($null -ne $errorHandled) {
            throw $errorHandled
        }
    }

    # Vérifier l'installation
    Write-Section "Vérification de l'installation"

    Write-Output "Vérification de l'installation de WinGet..."
    Start-Sleep -Seconds 3

    if (Get-WingetStatus -eq $true) {
        Write-Output "WinGet a été installé avec succès. Vous pouvez maintenant l'utiliser."
    } else {
        Write-Warning "WinGet est installé mais n'est pas détecté comme commande. Attendez environ 1 minute et réessayez. Un redémarrage peut être nécessaire."
    }

    if ($Wait) {
        Write-Output "Attente de $Seconds secondes avant de quitter..."
        Start-Sleep -Seconds 10
    }

    if ($NoExit) {
        Write-Output "Script terminé. Appuyez sur une touche pour quitter..."
        Read-Host
    }

    exit 0

} catch {
    Write-Section "ATTENTION ! Une erreur est survenue pendant l'installation !"
    
    if ($_.Exception.Message -notmatch '0x80073D02') {
        if ($Debug) {
            Write-Warning "Numéro de ligne : $($_.InvocationInfo.ScriptLineNumber)"
        }
        Write-Warning "Erreur : $($_.Exception.Message)"
    }
    
    exit 1
}
