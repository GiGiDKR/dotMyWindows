using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
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
        private const string DefaultPackagesContent = "[]";
        private static readonly string[] Branches = new string[] { "alpha", "dev", "main", "master" };
        private const string BaseUrlTemplate = "https://raw.githubusercontent.com/GiGiDKR/OhMyWindows/{0}/files/packages.json";
        private readonly string localPath;

        public InstallationService()
        {
            string baseDir = AppContext.BaseDirectory;
            localPath = Path.Combine(baseDir, "Data", "packages.json");
        }

        /// <summary>
        /// Télécharge ou charge localement le fichier packages.json.
        /// Tente les branches dans l'ordre jusqu'à réussite, ou utilise une version par défaut.
        /// </summary>
        public async Task InitializeAsync()
        {
            string content = null;
            using (HttpClient client = new HttpClient())
            {
                foreach (var branch in Branches)
                {
                    var url = string.Format(BaseUrlTemplate, branch);
                    try
                    {
                        content = await client.GetStringAsync(url);
                        if (!string.IsNullOrEmpty(content))
                        {
                            break;
                        }
                    }
                    catch
                    {
                        // Passer à la branche suivante en cas d'erreur.
                    }
                }
            }

            if (string.IsNullOrEmpty(content))
            {
                content = DefaultPackagesContent;
            }

            // Créer le dossier Data s'il n'existe pas
            var directory = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(localPath, content);
        }

        /// <summary>
        /// Lit et désérialise le fichier packages.json en une liste de packages.
        /// </summary>
        /// <returns>Une liste de Package.</returns>
        public async Task<List<Package>> GetPackagesAsync()
        {
            if (!File.Exists(localPath))
            {
                return new List<Package>();
            }

            string json = await File.ReadAllTextAsync(localPath);
            try
            {
                var packages = JsonSerializer.Deserialize<List<Package>>(json);
                return packages ?? new List<Package>();
            }
            catch
            {
                return new List<Package>();
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
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (package.Source.Equals("winget", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.FileName = "winget";
                    startInfo.Arguments = $"list --exact -q {package.Id}";
                }
                else if (package.Source.Equals("choco", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.FileName = "choco";
                    startInfo.Arguments = $"list --local-only --exact {package.Id}";
                }
                else
                {
                    return false;
                }

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync(cancellationToken);

                var output = await outputTask;
                return output.Contains(package.Id, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
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
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (package.Source.Equals("winget", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.FileName = "winget";
                    startInfo.Arguments = $"install {package.Id} --silent --accept-source-agreements --accept-package-agreements";
                }
                else if (package.Source.Equals("choco", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.FileName = "choco";
                    startInfo.Arguments = $"install {package.Id} -y";
                }
                else
                {
                    return false;
                }

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                await process.WaitForExitAsync(cancellationToken);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
