using System.Collections.Generic;
using System.Threading.Tasks;

namespace OhMyWindows.Contracts.Services;

public interface IInstalledPackagesService
{
    Task SetPackageInstalledAsync(string packageId, bool isInstalled);
    Task<bool> IsPackageInstalledAsync(string packageId);
    Task<Dictionary<string, bool>> LoadAsync();
}
