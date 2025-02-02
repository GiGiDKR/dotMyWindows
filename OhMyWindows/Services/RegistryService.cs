using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace OhMyWindows.Services;

public class RegistryService
{
    private bool _needsExplorerRestart;

    public async Task<bool> IsSettingEnabled(RegistryKeys.RegistryKey key)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var regKey = GetRegistryKey(key.Path, false);
                if (regKey == null)
                    return false;

                var value = regKey.GetValue(key.Name);
                if (value == null)
                    return false;

                var currentValue = value.ToString();
                System.Diagnostics.Debug.WriteLine($"Lecture de la clé {key.Path}\\{key.Name} : {currentValue} (Type: {regKey.GetValueKind(key.Name)})");
                
                return currentValue == key.EnableValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la lecture du registre : {ex.Message}");
                return false;
            }
        });
    }

    public async Task SetSetting(RegistryKeys.RegistryKey key, bool enable)
    {
        _needsExplorerRestart = false;

        await Task.Run(() =>
        {
            try
            {
                using var regKey = GetRegistryKey(key.Path, true);
                if (regKey == null)
                    throw new Exception($"Impossible de créer/ouvrir la clé : {key.Path}");

                var value = enable ? key.EnableValue : key.DisableValue;
                System.Diagnostics.Debug.WriteLine($"Écriture de la clé {key.Path}\\{key.Name} : {value} (Type: {key.ValueKind})");

                if (value == null)
                {
                    regKey.DeleteValue(key.Name, false);
                }
                else
                {
                    switch (key.ValueKind)
                    {
                        case RegistryValueKind.DWord:
                            if (!int.TryParse(value, out int intValue))
                                throw new Exception($"La valeur '{value}' n'est pas un nombre valide pour une clé DWORD");
                            regKey.SetValue(key.Name, intValue, key.ValueKind);
                            break;

                        case RegistryValueKind.String:
                            regKey.SetValue(key.Name, value, key.ValueKind);
                            break;

                        default:
                            throw new Exception($"Type de valeur non supporté : {key.ValueKind}");
                    }
                }

                // Vérifier que la valeur a bien été écrite
                var newValue = regKey.GetValue(key.Name)?.ToString();
                var newValueKind = regKey.GetValueKind(key.Name);
                System.Diagnostics.Debug.WriteLine($"Vérification de l'écriture : {newValue} (Type: {newValueKind})");

                if (newValue != value && value != null)
                {
                    throw new Exception($"La valeur n'a pas été correctement écrite dans le registre. Attendu : {value} ({key.ValueKind}), Obtenu : {newValue} ({newValueKind})");
                }

                // Vérifier si un redémarrage de l'explorateur est nécessaire
                if (key.Path.Contains("Explorer") || 
                    key.Path.Contains("TaskBar") || 
                    key.Path.Contains("Shell"))
                {
                    _needsExplorerRestart = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la modification du registre : {ex.Message}");
                throw;
            }
        });

        // Commenté temporairement
        /*if (_needsExplorerRestart)
        {
            await ShowExplorerRestartDialog();
        }*/
    }

    private async Task ShowExplorerRestartDialog()
    {
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Redémarrage nécessaire",
            Content = "Certains paramètres nécessitent un redémarrage de l'Explorateur Windows pour prendre effet. Voulez-vous redémarrer l'Explorateur maintenant ?",
            PrimaryButtonText = "Oui",
            SecondaryButtonText = "Non",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Secondary,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            await RestartExplorer();
        }
    }

    private async Task RestartExplorer()
    {
        await Task.Run(() =>
        {
            try
            {
                // Tuer le processus explorer.exe
                foreach (var process in System.Diagnostics.Process.GetProcessesByName("explorer"))
                {
                    process.Kill();
                }

                // L'explorateur redémarrera automatiquement
                // Mais on peut aussi le démarrer explicitement si nécessaire
                System.Diagnostics.Process.Start("explorer.exe");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du redémarrage de l'explorateur : {ex.Message}");
                throw;
            }
        });
    }

    public async Task<bool> RequiresElevatedPrivileges(RegistryKeys.RegistryKey key)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var regKey = GetRegistryKey(key.Path, true);
                return regKey == null;
            }
            catch
            {
                return true;
            }
        });
    }

    private Microsoft.Win32.RegistryKey GetRegistryKey(string path, bool writable)
    {
        try
        {
            // Déterminer la racine appropriée en fonction du chemin
            if (path.StartsWith("HKEY_CURRENT_USER\\") || path.StartsWith("HKCU\\"))
            {
                path = path.Replace("HKEY_CURRENT_USER\\", "").Replace("HKCU\\", "");
                return writable ? Registry.CurrentUser.CreateSubKey(path) : Registry.CurrentUser.OpenSubKey(path, writable)!;
            }
            else if (path.StartsWith("HKEY_LOCAL_MACHINE\\") || path.StartsWith("HKLM\\"))
            {
                path = path.Replace("HKEY_LOCAL_MACHINE\\", "").Replace("HKLM\\", "");
                return writable ? Registry.LocalMachine.CreateSubKey(path) : Registry.LocalMachine.OpenSubKey(path, writable)!;
            }
            else if (path.StartsWith("SOFTWARE\\"))
            {
                // Par défaut, essayer d'abord HKEY_CURRENT_USER
                var userPath = "SOFTWARE\\" + path.Replace("SOFTWARE\\", "");
                if (writable)
                {
                    return Registry.CurrentUser.CreateSubKey(userPath);
                }
                else
                {
                    var userKey = Registry.CurrentUser.OpenSubKey(userPath, writable);
                    if (userKey != null)
                        return userKey;

                    // Si pas trouvé dans HKEY_CURRENT_USER, essayer HKEY_LOCAL_MACHINE
                    return Registry.LocalMachine.OpenSubKey(path, writable)!;
                }
            }
            else
            {
                // Par défaut, utiliser HKEY_LOCAL_MACHINE
                return writable ? Registry.LocalMachine.CreateSubKey(path) : Registry.LocalMachine.OpenSubKey(path, writable)!;
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException("Cette opération nécessite des privilèges administrateur.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur lors de l'accès au registre : {ex.Message}");
        }
    }

    public async Task SetValue(RegistryKeys.RegistryKey key, int value)
    {
        await Task.Run(() =>
        {
            try
            {
                using var regKey = GetRegistryKey(key.Path, true);
                if (regKey == null)
                    throw new Exception($"Impossible de créer/ouvrir la clé : {key.Path}");

                System.Diagnostics.Debug.WriteLine($"Écriture de la valeur {value} dans la clé {key.Path}\\{key.Name}");
                regKey.SetValue(key.Name, value, RegistryValueKind.DWord);

                // Vérifier que la valeur a bien été écrite
                var newValue = regKey.GetValue(key.Name);
                if (newValue == null || (int)newValue != value)
                {
                    throw new Exception($"La valeur n'a pas été correctement écrite dans le registre. Attendu : {value}, Obtenu : {newValue}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la modification du registre : {ex.Message}");
                throw;
            }
        });
    }
}
