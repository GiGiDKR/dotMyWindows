using Microsoft.Extensions.Hosting;
using OhMyWindows.Services;
using OhMyWindows.ViewModels;

namespace OhMyWindows.App
{
    public partial class App : Application
    {
        private void ConfigureServices(HostBuilder host)
        {
            // Services
            host.Services.AddSingleton<InstallationService>();
            
            // ViewModels
            host.Services.AddTransient<InstallationViewModel>();
        }
    }
} 