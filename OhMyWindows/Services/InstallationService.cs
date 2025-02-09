using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OhMyWindows.Models;
using OhMyWindows.Contracts.Services;

namespace OhMyWindows.Services;
    public class InstallationService
    {
        private const string PackagesUrl = "https://raw.githubusercontent.com/GiGiDKR/OhMyWindows/alpha/files/packages.json";
        private readonly string _localPath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _logPath;
        private static readonly object _lockObj = new();
        private readonly PowerShellVersionService _powerShellVersionService;
        private readonly IInstalledPackagesService _installedPackagesService;

        private void LogToFile(string message)
        {
            try
            {
                lock (_lockObj)
                {
                    File.AppendAllText(_logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // Ignorer les erreurs de logging
            }
        }

        public InstallationService(IInstalledPackagesService installedPackagesService)
        {
            try
            {
                _installedPackagesService = installedPackagesService ?? throw new ArgumentNullException(nameof(installedPackagesService));
                
                // Utiliser LocalApplicationData au lieu de BaseDirectory pour les données
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OhMyWindows"
                );
                
                _logPath = Path.Combine(appDataPath, "logs", "installation_service.log");
                _localPath = Path.Combine(appDataPath, "data", "packages.json");
                
                // Créer les répertoires nécessaires
                var logDir = Path.GetDirectoryName(_logPath);
                var localDir = Path.GetDirectoryName(_localPath);
                if (!string.IsNullOrEmpty(logDir)) Directory.CreateDirectory(logDir);
                if (!string.IsNullOrEmpty(localDir)) Directory.CreateDirectory(localDir);
                
                LogToFile($"Répertoire de l'application : {appDataPath}");
                LogToFile($"Fichier de packages local : {_localPath}");

                _jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                };
                
                _powerShellVersionService = new PowerShellVersionService();
                LogToFile("PowerShellVersionService initialisé");
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur dans le constructeur : {ex.GetType().Name} - {ex.Message}");
                LogToFile($"StackTrace : {ex.StackTrace}");
                throw;
            }
        }

        private async Task<bool> TryDownloadPackages()
        {
            try
            {
                if (string.IsNullOrEmpty(_localPath))
                {
                    LogToFile("Erreur : _localPath n'est pas initialisé");
                    return false;
                }

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "OhMyWindows");
                LogToFile($"Tentative de téléchargement depuis : {PackagesUrl}");

                var response = await httpClient.GetAsync(PackagesUrl);
                LogToFile($"Statut de la réponse HTTP : {(int)response.StatusCode} {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    LogToFile($"Échec du téléchargement. Code : {response.StatusCode}");
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                LogToFile($"Contenu téléchargé depuis GitHub (longueur: {content.Length} caractères)");
                LogToFile($"Début du contenu : {content.Substring(0, Math.Min(100, content.Length))}...");

                try
                {
                    var packageList = JsonSerializer.Deserialize<PackageList>(content, _jsonOptions);
                    if (packageList?.Packages == null)
                    {
                        LogToFile("Le fichier JSON désérialisé contient une liste de packages null");
                        return false;
                    }
                    if (packageList.Packages.Count == 0)
                    {
                        LogToFile("Le fichier JSON désérialisé contient une liste de packages vide");
                        return false;
                    }

                    LogToFile($"Fichier GitHub valide avec {packageList.Packages.Count} packages");
                    
                    // Créer le répertoire si nécessaire
                    var directory = Path.GetDirectoryName(_localPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        LogToFile($"Création du répertoire : {directory}");
                    }

                    await File.WriteAllTextAsync(_localPath, content);
                    LogToFile($"Fichier sauvegardé localement dans : {_localPath}");
                    return true;
                }
                catch (JsonException ex)
                {
                    LogToFile($"Erreur de désérialisation JSON : {ex.Message}");
                    LogToFile($"Stack trace : {ex.StackTrace}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                LogToFile($"Erreur HTTP lors du téléchargement : {ex.Message}");
                if (ex.InnerException != null)
                    LogToFile($"Cause : {ex.InnerException.Message}");
                return false;
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur inattendue lors du téléchargement : {ex.Message}");
                LogToFile($"Type d'erreur : {ex.GetType().FullName}");
                LogToFile($"Stack trace : {ex.StackTrace}");
                return false;
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                LogToFile("Début de l'initialisation");

                if (await TryDownloadPackages())
                {
                    LogToFile("Fichier téléchargé avec succès");
                    return;
                }

                LogToFile("Échec du téléchargement, utilisation du fichier local");
                if (!File.Exists(_localPath))
                {
                    LogToFile("Le fichier local n'existe pas, création d'un fichier vide");
                    await File.WriteAllTextAsync(_localPath, JsonSerializer.Serialize(new PackageList { Packages = new List<Package>() }, _jsonOptions));
                    return;
                }

                var localContent = await File.ReadAllTextAsync(_localPath);
                try
                {
                    var packageList = JsonSerializer.Deserialize<PackageList>(localContent, _jsonOptions);
                    if (packageList?.Packages == null)
                    {
                        LogToFile("Le fichier local est invalide, création d'un fichier vide");
                        await File.WriteAllTextAsync(_localPath, JsonSerializer.Serialize(new PackageList { Packages = new List<Package>() }, _jsonOptions));
                    }
                }
                catch (JsonException ex)
                {
                    LogToFile($"Erreur de désérialisation du fichier local : {ex.Message}");
                    LogToFile("Création d'un fichier vide");
                    await File.WriteAllTextAsync(_localPath, JsonSerializer.Serialize(new PackageList { Packages = new List<Package>() }, _jsonOptions));
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur critique lors de l'initialisation : {ex}");
                throw;
            }
        }

        public async Task<List<Package>> GetPackagesAsync()
        {
            try
            {
                if (!File.Exists(_localPath))
                {
                    LogToFile("Le fichier packages.json n'existe pas, tentative de téléchargement...");
                    if (!await TryDownloadPackages())
                    {
                        LogToFile("Échec du téléchargement des packages");
                        return new List<Package>();
                    }
                }

                var content = await File.ReadAllTextAsync(_localPath);
                try
                {
                    var packageList = JsonSerializer.Deserialize<PackageList>(content, _jsonOptions);
                    if (packageList?.Packages == null)
                    {
                        LogToFile("Le fichier packages.json est invalide");
                        return new List<Package>();
                    }

                    LogToFile($"Chargement réussi de {packageList.Packages.Count} packages");
                    foreach (var package in packageList.Packages)
                    {
                        var isInstalled = await _installedPackagesService.IsPackageInstalledAsync(package.Id);
                        LogToFile($"Package {package.Name} - Installé: {isInstalled}");
                        
                        // Si le package est installé mais pas enregistré, on l'enregistre
                        if (isInstalled)
                        {
                            var wasRegistered = await _installedPackagesService.IsPackageInstalledAsync(package.Id);
                            if (!wasRegistered)
                            {
                                LogToFile($"Enregistrement du package {package.Name} comme installé");
                                await _installedPackagesService.SetPackageInstalledAsync(package.Id, true);
                            }
                        }
                        package.IsInstalled = isInstalled;
                    }

                    return packageList.Packages;
                }
                catch (JsonException ex)
                {
                    LogToFile($"Erreur de désérialisation : {ex.Message}");
                    if (File.Exists(_localPath))
                    {
                        LogToFile("Suppression du fichier corrompu et nouvelle tentative de téléchargement...");
                        File.Delete(_localPath);
                        if (await TryDownloadPackages())
                        {
                            return await GetPackagesAsync();
                        }
                    }
                    return new List<Package>();
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur lors de la lecture des packages : {ex}");
                return new List<Package>();
            }
        }

        public async Task<bool> IsPackageInstalledAsync(Package package, CancellationToken cancellationToken = default)
        {
            if (package == null) return false;

            try
            {
                var powerShellPath = PowerShellVersionService.GetLatestPowerShellVersion();
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = powerShellPath,
                    Arguments = "-NoProfile -NonInteractive -Command "
                };

                if (package.Source.Equals("winget", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.Arguments += $"\"& {{ winget list --exact -q '{package.Id}' | Out-String }}\"";
                }
                else if (package.Source.Equals("choco", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.Arguments += $"\"& {{ choco list '{package.Id}' | Out-String }}\"";
                }
                else
                {
                    return false;
                }

                LogToFile($"Vérification de {package.Name} avec la commande : {startInfo.FileName} {startInfo.Arguments}");

                using var process = Process.Start(startInfo) ??
                    throw new InvalidOperationException("Impossible de vérifier l'installation");

                var output = new System.Text.StringBuilder();
                var error = new System.Text.StringBuilder();

                process.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        error.AppendLine(e.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                var outputStr = output.ToString();
                var errorStr = error.ToString();

                LogToFile($"Sortie de la vérification pour {package.Name}:");
                LogToFile(outputStr);
                if (!string.IsNullOrEmpty(errorStr))
                {
                    LogToFile($"Erreurs lors de la vérification de {package.Name}:");
                    LogToFile(errorStr);
                }

                if (package.Source.Equals("winget", StringComparison.OrdinalIgnoreCase))
                {
                    return outputStr.Contains(package.Id, StringComparison.OrdinalIgnoreCase) &&
                           !outputStr.Contains("Aucun package installé", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    return process.ExitCode == 0 && outputStr.Contains(package.Id, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur lors de la vérification de {package.Name}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> InstallPackageAsync(Package package, CancellationToken cancellationToken = default)
        {
            if (package == null) return false;

            try
            {
                // Vérifier d'abord si le package est déjà installé
                if (await IsPackageInstalledAsync(package, cancellationToken))
                {
                    LogToFile($"Le package {package.Name} est déjà installé");
                    return true;
                }

                var powerShellPath = PowerShellVersionService.GetLatestPowerShellVersion();
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = powerShellPath,
                    Arguments = "-NoProfile -NonInteractive -Command "
                };

                if (package.Source.Equals("winget", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.Arguments += $"\"& {{ winget install '{package.Id}' --silent --accept-source-agreements --accept-package-agreements | Out-String }}\"";
                }
                else if (package.Source.Equals("choco", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.Arguments += $"\"& {{ choco install '{package.Id}' -y --no-progres| Out-String }}\"";
                }
                else
                {
                    return false;
                }

                LogToFile($"Installation de {package.Name} avec la commande : {startInfo.FileName} {startInfo.Arguments}");

                using var process = Process.Start(startInfo) ??
                    throw new InvalidOperationException("Impossible de démarrer le processus d'installation");

                var output = new System.Text.StringBuilder();
                var error = new System.Text.StringBuilder();

                process.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        LogToFile($"[Output] {e.Data}");
                        output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        LogToFile($"[Error] {e.Data}");
                        error.AppendLine(e.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode != 0)
                {
                    var errorMessage = error.ToString();
                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage = output.ToString();
                    }
                    throw new InvalidOperationException($"Échec de l'installation : {errorMessage}");
                }

                // Vérifier que le package est bien installé
                if (await IsPackageInstalledAsync(package, cancellationToken))
                {
                    await _installedPackagesService.SetPackageInstalledAsync(package.Id, true);
                    LogToFile($"Installation de {package.Name} réussie");
                    return true;
                }
                else
                {
                    LogToFile($"Le package {package.Name} n'est pas détecté après l'installation");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur lors de l'installation de {package.Name}: {ex.Message}");
                if (ex is OperationCanceledException)
                {
                    throw;
                }
                throw new InvalidOperationException($"Erreur lors de l'installation de {package.Name}: {ex.Message}", ex);
            }
        }
    }

public class PackageList
{
    public required List<Package> Packages { get; set; }
}
