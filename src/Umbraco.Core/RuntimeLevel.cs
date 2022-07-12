namespace Umbraco.Cms.Core;

/// <summary>
///     Describes the levels in which the runtime can run.
/// </summary>
public enum RuntimeLevel
{
    /// <summary>
    ///     The runtime has failed to boot and cannot run.
    /// </summary>
    BootFailed = -1,

    /// <summary>
    ///     The level is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     The runtime is booting.
    /// </summary>
    Boot = 1,

    /// <summary>
    ///     The runtime has detected that Umbraco is not installed at all, ie there is
    ///     no database, and is currently installing Umbraco.
    /// </summary>
    Install = 2,

    /// <summary>
    ///     The runtime has detected an Umbraco install which needed to be upgraded, and
    ///     is currently upgrading Umbraco.
    /// </summary>
    Upgrade = 3,

    /// <summary>
    ///     The runtime has detected an up-to-date Umbraco install and is running.
    /// </summary>
    Run = 100,
}
