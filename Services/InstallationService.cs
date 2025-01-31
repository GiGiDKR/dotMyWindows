using System.Net.Http;
using System.Text.Json;
using System.Diagnostics;
using OhMyWindows.Models;

namespace OhMyWindows.Services;

public class InstallationService
{
    private readonly HttpClient _httpClient;
    private const string PackagesUrl = "https://raw.githubusercontent.com/votre-repo/packages.json";
    private bool _isChecking = false;

    public InstallationService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<Package>> GetPackagesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(PackagesUrl);
            var packages = JsonSerializer.Deserialize<List<Package>>(response);
            return packages ?? new List<Package>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors du téléchargement des packages : {ex.Message}");
            return new List<Package>();
        }
    }

    public async Task<bool> IsPackageInstalledAsync(string packageId)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "winget",
                Arguments = $"list --id {packageId} --accept-source-agreements",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return false;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output.Contains(packageId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de la vérification du package {packageId} : {ex.Message}");
            return false;
        }
    }

    public async Task<bool> InstallPackageAsync(string packageId)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "winget",
                Arguments = $"install --id {packageId} --accept-source-agreements --accept-package-agreements",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de l'installation du package {packageId} : {ex.Message}");
            return false;
        }
    }

    public bool IsChecking
    {
        get => _isChecking;
        set => _isChecking = value;
    }
} 