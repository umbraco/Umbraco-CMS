namespace Umbraco.Cms.Core;

/// <summary>
///     Describes the reason for the runtime level.
/// </summary>
public enum RuntimeLevelReason
{
    /// <summary>
    ///     The reason is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    ///     The code version is lower than the version indicated in web.config, and
    ///     downgrading Umbraco is not supported.
    /// </summary>
    BootFailedCannotDowngrade,

    /// <summary>
    ///     The runtime cannot connect to the configured database.
    /// </summary>
    BootFailedCannotConnectToDatabase,

    /// <summary>
    ///     The runtime can connect to the configured database, but it cannot
    ///     retrieve the migrations status.
    /// </summary>
    BootFailedCannotCheckUpgradeState,

    /// <summary>
    ///     An exception was thrown during boot.
    /// </summary>
    BootFailedOnException,

    /// <summary>
    ///     Umbraco is not installed at all.
    /// </summary>
    InstallNoVersion,

    /// <summary>
    ///     A version is specified in web.config but the database is not configured.
    /// </summary>
    /// <remarks>This is a weird state.</remarks>
    InstallNoDatabase,

    /// <summary>
    ///     A version is specified in web.config and a database is configured, but the
    ///     database is missing, and installing over a missing database has been enabled.
    /// </summary>
    InstallMissingDatabase,

    /// <summary>
    ///     A version is specified in web.config and a database is configured, but the
    ///     database is empty, and installing over an empty database has been enabled.
    /// </summary>
    InstallEmptyDatabase,

    /// <summary>
    ///     Umbraco runs an old version.
    /// </summary>
    UpgradeOldVersion,

    /// <summary>
    ///     Umbraco runs the current version but some migrations have not run.
    /// </summary>
    UpgradeMigrations,

    /// <summary>
    ///     Umbraco runs the current version but some package migrations have not run.
    /// </summary>
    UpgradePackageMigrations,

    /// <summary>
    ///     Umbraco is running.
    /// </summary>
    Run,
}
