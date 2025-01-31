using CommunityToolkit.Mvvm.ComponentModel;

namespace OhMyWindows.Models;

public partial class Package : ObservableObject
{
    [ObservableProperty]
    private string id;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isInstalled;

    [ObservableProperty]
    private bool isChecking;

    public Package(string id, string name)
    {
        Id = id;
        Name = name;
        IsSelected = false;
        IsInstalled = false;
        IsChecking = false;
    }
} 