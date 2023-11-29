using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Packaging;

[DataContract(Name = "installedPackage")]
public class InstalledPackage
{
    [DataMember(Name = "id")]
    public string? PackageId { get; set; }

    [DataMember(Name = "name", IsRequired = true)]
    [Required]
    public string? PackageName { get; set; }

    [DataMember(Name = "packageView")]
    public string? PackageView { get; set; }

    [DataMember(Name = "version")]
    public string? Version { get; set; }

    [DataMember(Name = "allowPackageTelemetry")]
    public bool AllowPackageTelemetry { get; set; } = true;

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
