namespace OhMyWindows.Models;

public class InstalledProgram
{
    public required string Name { get; set; }
    public string? Version { get; set; }
    public string? Publisher { get; set; }
    public string? UninstallString { get; set; }
    public string? QuietUninstallString { get; set; }
    public string? InstallLocation { get; set; }
    public string? InstallDate { get; set; }
    public long EstimatedSize { get; set; }
}
