using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using OhMyWindows.Contracts.Services;

namespace OhMyWindows.Services;

public class InstalledPackagesService : IInstalledPackagesService
{
    private const string SettingsKey = "InstalledPackages";
    public required ILocalSettingsService LocalSettingsService { get; init; }
    private Dictionary<string, bool> _installedPackages;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly TaskCompletionSource _initializationTask = new();
    private readonly string _logPath;
    private static readonly object _lockObj = new();

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

    public InstalledPackagesService(ILocalSettingsService localSettingsService)
    {
        try
        {
            LocalSettingsService = localSettingsService ?? throw new ArgumentNullException(nameof(localSettingsService));
            _installedPackages = new Dictionary<string, bool>();
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OhMyWindows", "logs", "installed_packages.log");
            
            // Créer le répertoire des logs s'il n'existe pas
            var directoryPath = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            _logPath = logPath;

            LogToFile("Service initialisé");
            
            // Initialisation asynchrone avec meilleure gestion des erreurs
            Task.Run(async () =>
            {
                try
                {
                    LogToFile("Début du chargement des packages installés");
                    await LoadAsync();
                    LogToFile($"Chargement terminé, {_installedPackages.Count} packages trouvés");
                    _initializationTask.SetResult();
                }
                catch (Exception ex)
                {
                    LogToFile($"Erreur lors du chargement : {ex.Message}");
                    // En cas d'erreur, on initialise avec un dictionnaire vide mais on marque l'initialisation comme terminée
                    _installedPackages = new Dictionary<string, bool>();
                    _initializationTask.SetResult();
                }
            });
        }
        catch (Exception ex)
        {
            _logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OhMyWindows", "logs", "installed_packages.log");
            LogToFile($"Erreur dans le constructeur : {ex.Message}");
            _installedPackages = new Dictionary<string, bool>();
            _initializationTask.SetResult();
        }
    }

    public async Task SetPackageInstalledAsync(string packageId, bool isInstalled)
    {
        if (string.IsNullOrEmpty(packageId))
        {
            LogToFile("Tentative de définir un package avec un ID null ou vide");
            return;
        }

        await _initializationTask.Task; // Attendre l'initialisation
        await _semaphore.WaitAsync();
        try
        {
            LogToFile($"Définition du package {packageId} comme {(isInstalled ? "installé" : "désinstallé")}");
            _installedPackages[packageId] = isInstalled;
            await LocalSettingsService.SaveSettingAsync(SettingsKey, _installedPackages);
            LogToFile("Sauvegarde réussie");
        }
        catch (Exception ex)
        {
            LogToFile($"Erreur lors de la sauvegarde du package {packageId} : {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> IsPackageInstalledAsync(string packageId)
    {
        if (string.IsNullOrEmpty(packageId))
        {
            LogToFile("Vérification d'un package avec un ID null ou vide");
            return false;
        }

        await _initializationTask.Task; // Attendre l'initialisation
        var result = _installedPackages != null && _installedPackages.TryGetValue(packageId, out var isInstalled) && isInstalled;
        LogToFile($"Vérification du package {packageId} : {result}");
        return result;
    }

    public async Task<Dictionary<string, bool>> LoadAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            LogToFile("Début du chargement des packages");
            if (_installedPackages.Count == 0)
            {
                try 
                {
                    var loaded = await LocalSettingsService.ReadSettingAsync<Dictionary<string, bool>>(SettingsKey);
                    if (loaded != null)
                    {
                        _installedPackages = loaded;
                        LogToFile($"Chargement réussi : {loaded.Count} packages");
                    }
                    else
                    {
                        LogToFile("Aucun package trouvé dans les paramètres");
                    }
                }
                catch (Exception ex)
                {
                    LogToFile($"Erreur lors du chargement des paramètres : {ex.Message}");
                    // En cas d'erreur de chargement, on continue avec un dictionnaire vide
                    _installedPackages = new Dictionary<string, bool>();
                }
            }
            else
            {
                LogToFile($"Utilisation du cache existant : {_installedPackages.Count} packages");
            }
            return _installedPackages;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
