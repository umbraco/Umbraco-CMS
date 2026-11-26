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
        : this(semVersion, timeZoneInfo, runtimeMode, isDebugMode: false, deniedPermissions: Array.Empty<string>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerInformation"/> class.
    /// </summary>
    /// <param name="semVersion">The semantic version of the Umbraco installation.</param>
    /// <param name="timeZoneInfo">The time zone information for the server.</param>
    /// <param name="runtimeMode">The current runtime mode of the server.</param>
    /// <param name="isDebugMode">A value indicating whether the server is running in debug mode.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 21.")]
    public ServerInformation(SemVersion semVersion, TimeZoneInfo timeZoneInfo, RuntimeMode runtimeMode, bool isDebugMode)
        : this(semVersion, timeZoneInfo, runtimeMode, isDebugMode, Array.Empty<string>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerInformation"/> class.
    /// </summary>
    /// <param name="semVersion">The semantic version of the Umbraco installation.</param>
    /// <param name="timeZoneInfo">The time zone information for the server.</param>
    /// <param name="runtimeMode">The current runtime mode of the server.</param>
    /// <param name="isDebugMode">A value indicating whether the server is running in debug mode.</param>
    /// <param name="deniedPermissions">The permission verbs the environment denies.</param>
    public ServerInformation(
        SemVersion semVersion,
        TimeZoneInfo timeZoneInfo,
        RuntimeMode runtimeMode,
        bool isDebugMode,
        IReadOnlyCollection<string> deniedPermissions)
    {
        SemVersion = semVersion;
        TimeZoneInfo = timeZoneInfo;
        RuntimeMode = runtimeMode;
        IsDebugMode = isDebugMode;
        DeniedPermissions = deniedPermissions;
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

    /// <summary>
    /// Gets the permission verbs the environment denies, for example <c>Umb.DocumentType.Create</c>. Any verb not listed
    /// is permitted.
    /// </summary>
    public IReadOnlyCollection<string> DeniedPermissions { get; }
}
