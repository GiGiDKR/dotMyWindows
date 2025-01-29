using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace OhMyWindows.Services;

public class RegistryService
{
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

                return value.ToString() == key.EnableValue;
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
        await Task.Run(() =>
        {
            try
            {
                using var regKey = GetRegistryKey(key.Path, true);
                if (regKey == null)
                    throw new Exception($"Impossible de créer/ouvrir la clé : {key.Path}");

                var value = enable ? key.EnableValue : key.DisableValue;
                
                if (value == null)
                {
                    regKey.DeleteValue(key.Name, false);
                }
                else
                {
                    regKey.SetValue(key.Name, value, RegistryValueKind.String);
                }

                // Notifier Windows des changements si nécessaire
                if (key.Path.Contains("Explorer"))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la modification du registre : {ex.Message}");
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
} 