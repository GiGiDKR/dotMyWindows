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

namespace OhMyWindows.Services
{
    /// <summary>
    /// Service pour gérer le téléchargement et la lecture du fichier packages.json.
    /// Tente de télécharger packages.json depuis GitHub en testant les branches "alpha", "dev", "main", "master".
    /// Si le téléchargement échoue, une version par défaut (vide) sera utilisée.
    /// Le fichier est sauvegardé localement dans le dossier Data.
    /// </summary>
    public class InstallationService
    {
        private const string PackagesUrl = "https://raw.githubusercontent.com/GiGiDKR/OhMyWindows/alpha/files/packages.json";
        private readonly string _localPath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _logPath;
        private static readonly object _lockObj = new object();

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

        public InstallationService()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _logPath = Path.Combine(baseDir, "installation_service.log");
                LogToFile($"Répertoire de base de l'application : {baseDir}");

                var dataPath = Path.Combine(baseDir, "Data");
                LogToFile($"Chemin du dossier Data : {dataPath}");
                
                if (!Directory.Exists(dataPath))
                {
                    LogToFile("Création du dossier Data...");
                    Directory.CreateDirectory(dataPath);
                    LogToFile("Dossier Data créé avec succès");
                }

                _localPath = Path.Combine(dataPath, "packages.json");
                LogToFile($"Chemin du fichier packages.json : {_localPath}");

                _jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Converters = { new JsonStringEnumConverter() }
                };
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
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "OhMyWindows");
                LogToFile($"Tentative de téléchargement depuis : {PackagesUrl}");

                var response = await httpClient.GetAsync(PackagesUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    LogToFile("Contenu téléchargé depuis GitHub");

                    try
                    {
                        var packageList = JsonSerializer.Deserialize<PackageList>(content, _jsonOptions);
                        if (packageList?.Packages != null && packageList.Packages.Count > 0)
                        {
                            LogToFile($"Fichier GitHub valide avec {packageList.Packages.Count} packages");
                            
                            // Assigner une catégorie par défaut si nécessaire
                            foreach (var package in packageList.Packages)
                            {
                                if (string.IsNullOrWhiteSpace(package.Category))
                                {
                                    package.Category = "Autres";
                                    LogToFile($"Catégorie par défaut assignée pour : {package.Name}");
                                }
                            }

                            var formattedJson = JsonSerializer.Serialize(packageList, _jsonOptions);
                            await File.WriteAllTextAsync(_localPath, formattedJson);
                            return true;
                        }
                        LogToFile("Le fichier ne contient pas de packages valides");
                    }
                    catch (JsonException ex)
                    {
                        LogToFile($"Erreur de désérialisation JSON : {ex.Message}");
                    }
                }
                else
                {
                    LogToFile($"Échec du téléchargement. Code : {response.StatusCode}");
                }
                return false;
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur lors du téléchargement : {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Télécharge ou charge localement le fichier packages.json.
        /// Tente les branches dans l'ordre jusqu'à réussite, ou utilise une version par défaut.
        /// </summary>
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
                    throw new FileNotFoundException("Le fichier packages.json n'existe pas et n'a pas pu être téléchargé.");
                }

                var localContent = await File.ReadAllTextAsync(_localPath);
                var packageList = JsonSerializer.Deserialize<PackageList>(localContent, _jsonOptions);
                if (packageList?.Packages == null || packageList.Packages.Count == 0)
                {
                    throw new InvalidOperationException("Le fichier packages.json est invalide ou vide");
                }

                LogToFile($"Utilisation du fichier local avec {packageList.Packages.Count} packages");
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur lors de l'initialisation : {ex}");
                throw;
            }
        }

        /// <summary>
        /// Lit et désérialise le fichier packages.json en une liste de packages.
        /// </summary>
        /// <returns>Une liste de Package.</returns>
        public async Task<List<Package>> GetPackagesAsync()
        {
            try
            {
                LogToFile("Début du chargement des packages");
                var jsonContent = await File.ReadAllTextAsync(_localPath);

                var packageList = JsonSerializer.Deserialize<PackageList>(jsonContent, _jsonOptions) ??
                    throw new InvalidOperationException("Le fichier packages.json est invalide");

                LogToFile($"Packages chargés : {packageList.Packages.Count}");
                return packageList.Packages;
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur lors du chargement des packages : {ex}");
                throw;
            }
        }

        /// <summary>
        /// Vérifie si un package est installé en utilisant winget ou choco selon la source.
        /// </summary>
        public async Task<bool> IsPackageInstalledAsync(Package package, CancellationToken cancellationToken = default)
        {
            if (package == null) return false;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = "/c "
                };

                if (package.Source.Equals("winget", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.Arguments += $"winget list --exact -q {package.Id}";
                }
                else if (package.Source.Equals("choco", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.Arguments += $"choco list --local-only --exact {package.Id}";
                }
                else
                {
                    return false;
                }

                using var process = Process.Start(startInfo) ??
                    throw new InvalidOperationException("Impossible de vérifier l'installation");

                await process.WaitForExitAsync(cancellationToken);
                var output = await process.StandardOutput.ReadToEndAsync();
                return output.Contains(package.Id, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur lors de la vérification de {package.Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Installe un package en utilisant winget ou choco selon la source.
        /// </summary>
        public async Task<bool> InstallPackageAsync(Package package, CancellationToken cancellationToken = default)
        {
            if (package == null) return false;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = "/c "
                };

                if (package.Source.Equals("winget", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.Arguments += $"winget install {package.Id} --silent --accept-source-agreements --accept-package-agreements";
                }
                else if (package.Source.Equals("choco", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.Arguments += $"choco install {package.Id} -y";
                }
                else
                {
                    return false;
                }

                using var process = Process.Start(startInfo) ??
                    throw new InvalidOperationException("Impossible de démarrer le processus d'installation");

                try
                {
                    await process.WaitForExitAsync(cancellationToken);
                    return process.ExitCode == 0;
                }
                catch (OperationCanceledException)
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur lors de l'installation de {package.Name}: {ex.Message}");
                if (ex is OperationCanceledException)
                {
                    throw;
                }
                return false;
            }
        }
    }

    public class PackageList
    {
        public required List<Package> Packages { get; set; }
    }
}
