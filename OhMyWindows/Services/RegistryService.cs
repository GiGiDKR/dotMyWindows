using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OhMyWindows.Services;

public class RegistryService
{
    private readonly string _regFilesPath;

    public RegistryService()
    {
        _regFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files", "Reg");
    }

    public async Task ApplyRegFile(string category, string setting, bool enable)
    {
        var filePath = Path.Combine(_regFilesPath, category, $"{setting}-{(enable ? "Enable" : "Disable")}.reg");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Le fichier .reg n'a pas été trouvé : {filePath}");
        }

        await Task.Run(() =>
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "reg";
            process.StartInfo.Arguments = $"import \"{filePath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Erreur lors de l'application du fichier .reg : {filePath}");
            }
        });
    }

    public async Task<bool> IsSettingEnabled(string category, string setting)
    {
        // TODO: Implémenter la lecture du registre pour vérifier l'état actuel du paramètre
        await Task.CompletedTask;
        return false;
    }

    public class RegistryException : Exception
    {
        public RegistryException(string message) : base(message)
        {
        }

        public RegistryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 