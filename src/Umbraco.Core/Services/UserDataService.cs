using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

[Obsolete($"Use {nameof(ISystemTroubleshootingInformationService)} instead. Will be removed in V16.")]
public class UserDataService : IUserDataService
{
    private readonly ISystemTroubleshootingInformationService _systemTroubleshootingInformationService;

    [Obsolete($"Use the constructor that accepts {nameof(ISystemTroubleshootingInformationService)}")]
    public UserDataService(IUmbracoVersion version, ILocalizationService localizationService)
        : this(version, localizationService, StaticServiceProvider.Instance.GetRequiredService<ISystemTroubleshootingInformationService>())
    {
    }

    public UserDataService(IUmbracoVersion version, ILocalizationService localizationService, ISystemTroubleshootingInformationService systemTroubleshootingInformationService)
        => _systemTroubleshootingInformationService = systemTroubleshootingInformationService;

    [Obsolete($"Use {nameof(ISystemTroubleshootingInformationService)} instead. Will be removed in V16.")]
    public IEnumerable<UserData> GetUserData() =>
        _systemTroubleshootingInformationService.GetTroubleshootingInformation().Select(kvp => new UserData(kvp.Key, kvp.Value)).ToArray();

    public bool IsRunningInProcessIIS()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        var processName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
        return processName.Contains("w3wp") || processName.Contains("iisexpress");
    }
}
