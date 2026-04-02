namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an installation operation.
/// </summary>
public enum InstallOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified database provider is unknown or not supported.
    /// </summary>
    UnknownDatabaseProvider,

    /// <summary>
    ///     The database connection string is missing.
    /// </summary>
    MissingConnectionString,

    /// <summary>
    ///     The database provider name is missing.
    /// </summary>
    MissingProviderName,

    /// <summary>
    ///     The database connection could not be established.
    /// </summary>
    DatabaseConnectionFailed,

    /// <summary>
    ///     The installation process failed.
    /// </summary>
    InstallFailed,
}
