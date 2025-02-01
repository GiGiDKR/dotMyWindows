using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace OhMyWindows.Services
{
    public class InstalledPackagesService
    {
        private readonly string _filePath;
        private HashSet<string> _installedPackages;

        public InstalledPackagesService()
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, "Data", "installed_packages.json");
            _installedPackages = new HashSet<string>();
            LoadAsync().Wait();
        }

        /// <summary>
        /// Marque un package comme installé et sauvegarde l'état de manière asynchrone.
        /// </summary>
        /// <param name="packageId">L'identifiant du package.</param>
        /// <param name="isInstalled">Indique si le package est installé ou non.</param>
        public async Task SetPackageInstalledAsync(string packageId, bool isInstalled)
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

        /// <summary>
        /// Vérifie si un package est marqué comme installé.
        /// </summary>
        /// <param name="packageId">L'identifiant du package.</param>
        /// <returns>true si le package est installé, sinon false.</returns>
        public bool IsPackageInstalled(string packageId)
        {
            return _installedPackages.Contains(packageId);
        }

        /// <summary>
        /// Charge l'état des packages installés depuis le fichier JSON de manière asynchrone.
        /// </summary>
        private async Task LoadAsync()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(_filePath);
                    var packages = JsonSerializer.Deserialize<HashSet<string>>(json);
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

        /// <summary>
        /// Sauvegarde l'état des packages installés dans le fichier JSON de manière asynchrone.
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                // Créer le dossier Data s'il n'existe pas
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_installedPackages, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception)
            {
                // Gérer l'erreur selon les besoins.
            }
        }
    }
}
