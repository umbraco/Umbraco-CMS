using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Models;

public class ServerInformation(SemVersion semVersion, TimeZoneInfo timeZoneInfo, RuntimeMode runtimeMode)
{
    public SemVersion SemVersion { get; } = semVersion;
    public TimeZoneInfo TimeZoneInfo { get; } = timeZoneInfo;
    public RuntimeMode RuntimeMode { get; } = runtimeMode;
}
