using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Represents an installed Umbraco package with its metadata and migration status.
/// </summary>
[DataContract(Name = "installedPackage")]
public class InstalledPackage
{
    /// <summary>
    ///     Gets or sets the unique identifier of the package.
    /// </summary>
    [DataMember(Name = "id")]
    public string? PackageId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the package.
    /// </summary>
    [DataMember(Name = "name", IsRequired = true)]
    [Required]
    public string? PackageName { get; set; }

    /// <summary>
    ///     Gets or sets the custom view path for the package in the back office.
    /// </summary>
    [DataMember(Name = "packageView")]
    public string? PackageView { get; set; }

    /// <summary>
    ///     Gets or sets the version of the installed package.
    /// </summary>
    [DataMember(Name = "version")]
    public string? Version { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the package allows telemetry data collection.
    /// </summary>
    [DataMember(Name = "allowPackageTelemetry")]
    public bool AllowPackageTelemetry { get; set; } = true;

    /// <summary>
    ///     Gets or sets the collection of migration plans associated with this package.
    /// </summary>
    [DataMember(Name = "plans")]
    public IEnumerable<InstalledPackageMigrationPlans> PackageMigrationPlans { get; set; } = Enumerable.Empty<InstalledPackageMigrationPlans>();

    /// <summary>
    /// Gets a value indicating whether this package has migrations.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this package has migrations; otherwise, <c>false</c>.
    /// </value>
    [DataMember(Name = "hasMigrations")]
    public bool HasMigrations => PackageMigrationPlans.Any();

    /// <summary>
    /// Gets a value indicating whether this package has pending migrations.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this package has pending migrations; otherwise, <c>false</c>.
    /// </value>
    [DataMember(Name = "hasPendingMigrations")]
    public bool HasPendingMigrations => PackageMigrationPlans.Any(x => x.HasPendingMigrations);
}
