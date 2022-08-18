using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Packaging;

[DataContract(Name = "installedPackage")]
public class InstalledPackage
{
    [DataMember(Name = "name", IsRequired = true)]
    [Required]
    public string? PackageName { get; set; }

    // TODO: Version? Icon? Other metadata? This would need to come from querying the package on Our
    [DataMember(Name = "packageView")]
    public string? PackageView { get; set; }

    [DataMember(Name = "version")]
    public string? Version { get; set; }

    [DataMember(Name = "plans")]
    public IEnumerable<InstalledPackageMigrationPlans> PackageMigrationPlans { get; set; } =
        Enumerable.Empty<InstalledPackageMigrationPlans>();

    /// <summary>
    ///     It the package contains any migrations at all
    /// </summary>
    [DataMember(Name = "hasMigrations")]
    public bool HasMigrations => PackageMigrationPlans.Any();

    /// <summary>
    ///     If the package has any pending migrations to run
    /// </summary>
    [DataMember(Name = "hasPendingMigrations")]
    public bool HasPendingMigrations => PackageMigrationPlans.Any(x => x.HasPendingMigrations);
}
