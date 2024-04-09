namespace Umbraco.Cms.Api.Management.ViewModels.Package;

public class PackageMigrationStatusResponseModel
{
    /// <summary>
    ///     Gets or sets the name of the package.
    /// </summary>
    public required string PackageName { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the package has any pending migrations to run.
    /// </summary>
    public bool HasPendingMigrations { get; set; }
}
