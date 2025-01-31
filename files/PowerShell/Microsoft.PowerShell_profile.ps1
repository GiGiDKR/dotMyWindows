# Imports the terminal Icons 
Import-Module -Name Terminal-Icons

# Initialize Oh My Posh
oh-my-posh init pwsh --config "$env:POSH_THEMES_PATH\night-owl.omp.json" | Invoke-Expression

# PSReadline options
Import-Module PSReadLine
Set-PSReadlineKeyHandler -Key Tab -Function MenuComplete
Set-PSReadlineKeyHandler -Key UpArrow -Function HistorySearchBackward
Set-PSReadlineKeyHandler -Key DownArrow -Function HistorySearchForward
Set-PSReadLineOption -PredictionSource History
Set-PSReadLineOption -PredictionViewStyle ListView
Set-PSReadLineOption -EditMode Windows

# Shortcut for searching
Import-Module PSFzf
Set-PsFzfOption -PSReadlineChordProvider 'Ctrl+f' -PSReadlineChordReverseHistory 'Ctrl+r'

# Absolute path of commandlets
function which ($command) { 
  Get-Command -Name $command -ErrorAction SilentlyContinue | 
  Select-Object -ExpandProperty Path -ErrorAction SilentlyContinue 
}

# Set some Alias
Set-Alias n notepad
Set-Alias c clear
Set-Alias l ls
Set-Alias g git

# Onefetch
# git repository greeter
$global:lastRepository = $null

function Check-DirectoryForNewRepository {
    $currentRepository = git rev-parse --show-toplevel 2>$null
    if ($currentRepository -and ($currentRepository -ne $global:lastRepository)) {
        onefetch
    }
    $global:lastRepository = $currentRepository
}

function Set-Location {
    param (
        [string]$path
    )

    # Use the default Set-Location to change the directory
    Microsoft.PowerShell.Management\Set-Location -Path $path

    # Check if we are in a new Git repository
    Check-DirectoryForNewRepository
}

# Optional: Check the repository also when opening a shell directly in a repository directory
# Uncomment the following line if desired
#Check-DirectoryForNewRepository

# Zoxide
Invoke-Expression (& { (zoxide init powershell | Out-String) })