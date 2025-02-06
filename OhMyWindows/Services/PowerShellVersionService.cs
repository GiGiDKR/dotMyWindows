using System;
using System.Diagnostics;
using System.Management.Automation;

namespace OhMyWindows.Services;

public class PowerShellVersionService
{
    public static string GetLatestPowerShellVersion()
    {
        var ps7Path = @"C:\Program Files\PowerShell\7\pwsh.exe";
        var ps5Path = @"C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe";

        if (System.IO.File.Exists(ps7Path))
        {
            return ps7Path;
        }
        else if (System.IO.File.Exists(ps5Path))
        {
            return ps5Path;
        }
        else
        {
            return "cmd.exe";
        }
    }
}
