namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Specifies the local temporary storage location.
/// </summary>
public enum LocalTempStorage
{
    /// <summary>
    ///     The storage location is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     Use the default storage location within the application directory.
    /// </summary>
    Default,

    /// <summary>
    ///     Use the system's environment temporary directory.
    /// </summary>
    EnvironmentTemp,
}
