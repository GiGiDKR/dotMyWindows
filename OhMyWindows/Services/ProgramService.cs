using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace OhMyWindows.Services;

public class InstalledProgram
{
    public required string Name { get; set; }
    public string? Version { get; set; }
    public string? Publisher { get; set; }
    public string? UninstallString { get; set; }
    public string? QuietUninstallString { get; set; }
    public string? InstallLocation { get; set; }
    public string? InstallDate { get; set; }
    public long EstimatedSize { get; set; }
    public bool IsSelected { get; set; }
}

public class ProgramService
{
    private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    private const string Wow64UninstallKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

    public async Task<List<InstalledProgram>> GetInstalledProgramsAsync()
    {
        var programs = new List<InstalledProgram>();

        await Task.Run(() =>
        {
            // Recherche dans les clés de registre 64 bits
            using (var key = Registry.LocalMachine.OpenSubKey(UninstallKey))
            {
                if (key != null)
                {
                    programs.AddRange(GetProgramsFromRegistryKey(key));
                }
            }

            // Recherche dans les clés de registre 32 bits
            using (var key = Registry.LocalMachine.OpenSubKey(Wow64UninstallKey))
            {
                if (key != null)
                {
                    programs.AddRange(GetProgramsFromRegistryKey(key));
                }
            }
        });

        return programs.OrderBy(p => p.Name).ToList();
    }

    private IEnumerable<InstalledProgram> GetProgramsFromRegistryKey(RegistryKey key)
    {
        foreach (var subKeyName in key.GetSubKeyNames())
        {
            using var subKey = key.OpenSubKey(subKeyName);
            if (subKey == null) continue;

            var displayName = subKey.GetValue("DisplayName") as string;
            if (string.IsNullOrWhiteSpace(displayName)) continue;

            var estimatedSizeValue = subKey.GetValue("EstimatedSize");
            var estimatedSize = estimatedSizeValue != null ? Convert.ToInt64(estimatedSizeValue) : 0;
            
            yield return new InstalledProgram
            {
                Name = displayName,
                Version = subKey.GetValue("DisplayVersion") as string ?? "Version inconnue",
                Publisher = subKey.GetValue("Publisher") as string ?? "Éditeur inconnu",
                UninstallString = subKey.GetValue("UninstallString") as string ?? "",
                QuietUninstallString = subKey.GetValue("QuietUninstallString") as string ?? "",
                InstallLocation = subKey.GetValue("InstallLocation") as string ?? "",
                InstallDate = subKey.GetValue("InstallDate") as string ?? "",
                EstimatedSize = estimatedSize,
                IsSelected = false
            };
        }
    }

    public async Task<bool> UninstallProgramAsync(InstalledProgram program)
    {
        if (string.IsNullOrWhiteSpace(program.UninstallString))
            return false;

        try
        {
            var uninstallString = program.UninstallString.Trim('"');
            var match = Regex.Match(uninstallString, @"^(.*?.exe).*?$");
            var exePath = match.Success ? match.Groups[1].Value : uninstallString;
            var arguments = match.Success ? uninstallString.Substring(match.Groups[1].Value.Length).Trim() : "";

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments + " /quiet",
                UseShellExecute = true,
                Verb = "runas"
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de la désinstallation : {ex.Message}");
        }

        return false;
    }

    public async Task<bool> InstallProgramAsync(string programPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = programPath,
                UseShellExecute = true,
                Verb = "runas",
                Arguments = "/quiet"
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de l'installation : {ex.Message}");
        }

        return false;
    }

    public async Task<bool> InstallWithWingetAsync(string packageId)
    {
        try
        {
            using var powerShell = PowerShell.Create();
            powerShell.AddCommand("winget")
                     .AddArgument("install")
                     .AddArgument(packageId)
                     .AddArgument("--silent");

            var result = await powerShell.InvokeAsync();
            return !powerShell.HadErrors;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de l'installation avec Winget : {ex.Message}");
            return false;
        }
    }
}
