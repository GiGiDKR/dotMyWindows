using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OhMyWindows.Services;

public class PackageManagerInstallationService
{
    private readonly string _scriptsPath;

    public PackageManagerInstallationService()
    {
        _scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");
        Directory.CreateDirectory(_scriptsPath);
    }

    public async Task InstallWinGetAsync()
    {
        var scriptPath = Path.Combine(_scriptsPath, "WinGetInstall.ps1");
        await RunPowerShellScriptAsync(scriptPath);
    }

    public async Task InstallChocolateyAsync()
    {
        var command = @"Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))";
        await RunPowerShellCommandAsync(command);
    }

    public async Task InstallScoopAsync()
    {
        var command = "iwr -useb get.scoop.sh | iex";
        await RunPowerShellCommandAsync(command);
    }

    public async Task InstallPipAsync()
    {
        var command = @"[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri https://bootstrap.pypa.io/get-pip.py -OutFile get-pip.py; python get-pip.py";
        await RunPowerShellCommandAsync(command);
    }

    private async Task RunPowerShellScriptAsync(string scriptPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        await RunProcessAsync(startInfo);
    }

    private async Task RunPowerShellCommandAsync(string command)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        await RunProcessAsync(startInfo);
    }

    private async Task RunProcessAsync(ProcessStartInfo startInfo)
    {
        using var process = new Process { StartInfo = startInfo };
        
        process.Start();
        
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new Exception($"Erreur lors de l'exécution du script : {error}");
        }
    }
} 