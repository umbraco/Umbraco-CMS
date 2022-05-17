using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Packaging;

[DataContract(Name = "installedPackageMigrations")]
public class InstalledPackageMigrationPlans
{
    [DataMember(Name = "hasPendingMigrations")]
    public bool HasPendingMigrations => FinalMigrationId != CurrentMigrationId;

    /// <summary>
    ///     If the package has migrations, this will be it's final migration Id
    /// </summary>
    /// <remarks>
    ///     This can be used to determine if the package advertises any migrations
    /// </remarks>
    [DataMember(Name = "finalMigrationId")]
    public string? FinalMigrationId { get; set; }

    /// <summary>
    ///     If the package has migrations, this will be it's current migration Id
    /// </summary>
    [DataMember(Name = "currentMigrationId")]
    public string? CurrentMigrationId { get; set; }
}
