using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading;

namespace OhMyWindows.Services
{
    public class InstalledPackagesService
    {
        private readonly string _filePath;
        private HashSet<string> _installedPackages;
        private readonly JsonSerializerOptions _jsonOptions;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly TaskCompletionSource _initializationTask = new();

        public InstalledPackagesService()
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, "Data", "installed_packages.json");
            _installedPackages = new HashSet<string>();
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            
            // Initialisation asynchrone
            Task.Run(async () =>
            {
                try
                {
                    await LoadAsync();
                    _initializationTask.SetResult();
                }
                catch (Exception ex)
                {
                    _initializationTask.SetException(ex);
                }
            });
        }

        /// <summary>
        /// Marque un package comme installé et sauvegarde l'état de manière asynchrone.
        /// </summary>
        /// <param name="packageId">L'identifiant du package.</param>
        /// <param name="isInstalled">Indique si le package est installé ou non.</param>
        public async Task SetPackageInstalledAsync(string packageId, bool isInstalled)
        {
            await _initializationTask.Task; // Attendre l'initialisation
            await _semaphore.WaitAsync();
            try
            {
                bool changed = false;
                if (isInstalled)
                {
                    changed = _installedPackages.Add(packageId);
                }
                else
                {
                    changed = _installedPackages.Remove(packageId);
                }

                if (changed)
                {
                    await SaveAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Vérifie si un package est marqué comme installé.
        /// </summary>
        /// <param name="packageId">L'identifiant du package.</param>
        /// <returns>true si le package est installé, sinon false.</returns>
        public async Task<bool> IsPackageInstalledAsync(string packageId)
        {
            await _initializationTask.Task; // Attendre l'initialisation
            return _installedPackages.Contains(packageId);
        }

        /// <summary>
        /// Charge l'état des packages installés depuis le fichier JSON de manière asynchrone.
        /// </summary>
        private async Task LoadAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (File.Exists(_filePath))
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(_filePath);
                        var packages = JsonSerializer.Deserialize<HashSet<string>>(json, _jsonOptions);
                        if (packages != null)
                        {
                            _installedPackages = packages;
                        }
                    }
                    catch (Exception)
                    {
                        // En cas d'erreur, initialise avec un ensemble vide.
                        _installedPackages = new HashSet<string>();
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Sauvegarde l'état des packages installés dans le fichier JSON de manière asynchrone.
        /// </summary>
        private async Task SaveAsync()
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_installedPackages, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
