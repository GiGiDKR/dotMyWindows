using CommunityToolkit.Mvvm.ComponentModel;

namespace OhMyWindows.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public string Title => "À propos";

    public AboutViewModel()
    {
    }
} 