using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents information about the Umbraco server instance.
/// </summary>
public class ServerInformation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerInformation"/> class.
    /// </summary>
    /// <param name="semVersion">The semantic version of the Umbraco installation.</param>
    /// <param name="timeZoneInfo">The time zone information for the server.</param>
    /// <param name="runtimeMode">The current runtime mode of the server.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ServerInformation(SemVersion semVersion, TimeZoneInfo timeZoneInfo, RuntimeMode runtimeMode)
        : this(semVersion, timeZoneInfo, runtimeMode, isDebugMode: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerInformation"/> class.
    /// </summary>
    /// <param name="semVersion">The semantic version of the Umbraco installation.</param>
    /// <param name="timeZoneInfo">The time zone information for the server.</param>
    /// <param name="runtimeMode">The current runtime mode of the server.</param>
    /// <param name="isDebugMode">A value indicating whether the server is running in debug mode.</param>
    public ServerInformation(SemVersion semVersion, TimeZoneInfo timeZoneInfo, RuntimeMode runtimeMode, bool isDebugMode)
    {
        SemVersion = semVersion;
        TimeZoneInfo = timeZoneInfo;
        RuntimeMode = runtimeMode;
        IsDebugMode = isDebugMode;
    }

    /// <summary>
    /// Gets the semantic version of the Umbraco installation.
    /// </summary>
    public SemVersion SemVersion { get; }

    /// <summary>
    /// Gets the time zone information for the server.
    /// </summary>
    public TimeZoneInfo TimeZoneInfo { get; }

    /// <summary>
    /// Gets the current runtime mode of the server.
    /// </summary>
    public RuntimeMode RuntimeMode { get; }

    /// <summary>
    /// Gets a value indicating whether the server is running in debug mode.
    /// </summary>
    public bool IsDebugMode { get; }
}
