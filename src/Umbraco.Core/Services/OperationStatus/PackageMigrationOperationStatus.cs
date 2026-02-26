namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a package migration operation.
/// </summary>
public enum PackageMigrationOperationStatus
{
    /// <summary>
    /// The package migration completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The specified package migration was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The operation was cancelled because a migration step failed.
    /// </summary>
    CancelledByFailedMigration
}
