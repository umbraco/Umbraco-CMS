using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents information about the Umbraco server instance.
/// </summary>
/// <param name="semVersion">The semantic version of the Umbraco installation.</param>
/// <param name="timeZoneInfo">The time zone information for the server.</param>
/// <param name="runtimeMode">The current runtime mode of the server.</param>
public class ServerInformation(SemVersion semVersion, TimeZoneInfo timeZoneInfo, RuntimeMode runtimeMode)
{
    /// <summary>
    /// Gets the semantic version of the Umbraco installation.
    /// </summary>
    public SemVersion SemVersion { get; } = semVersion;

    /// <summary>
    /// Gets the time zone information for the server.
    /// </summary>
    public TimeZoneInfo TimeZoneInfo { get; } = timeZoneInfo;

    /// <summary>
    /// Gets the current runtime mode of the server.
    /// </summary>
    public RuntimeMode RuntimeMode { get; } = runtimeMode;
}
