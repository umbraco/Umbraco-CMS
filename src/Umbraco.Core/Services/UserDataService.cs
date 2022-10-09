using System.Diagnostics;
using System.Runtime.InteropServices;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

[Obsolete("Use the IUserDataService interface instead")]
public class UserDataService : IUserDataService
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoVersion _version;

    public UserDataService(IUmbracoVersion version, ILocalizationService localizationService)
    {
        _version = version;
        _localizationService = localizationService;
    }

    public IEnumerable<UserData> GetUserData() =>
        new List<UserData>
        {
            new("Server OS", RuntimeInformation.OSDescription),
            new("Server Framework", RuntimeInformation.FrameworkDescription),
            new("Default Language", _localizationService.GetDefaultLanguageIsoCode()),
            new("Umbraco Version", _version.SemanticVersion.ToSemanticStringWithoutBuild()),
            new("Current Culture", Thread.CurrentThread.CurrentCulture.ToString()),
            new("Current UI Culture", Thread.CurrentThread.CurrentUICulture.ToString()),
            new("Current Webserver", GetCurrentWebServer()),
        };

    public bool IsRunningInProcessIIS()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        var processName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
        return processName.Contains("w3wp") || processName.Contains("iisexpress");
    }

    private string GetCurrentWebServer() => IsRunningInProcessIIS() ? "IIS" : "Kestrel";
}
