using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents the data transfer object (DTO) for the schema of a package that has been created in the persistence layer.
/// </summary>
[TableName(TableName)]
[ExplicitColumns]
[PrimaryKey(PrimaryKeyColumnName)]
public class CreatedPackageSchemaDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.CreatedPackageSchema;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string NameColumnName = "name";

    /// <summary>
    /// Gets or sets the unique identifier for the created package schema.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the package schema.
    /// </summary>
    [Column(NameColumnName)]
    [Length(255)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = NameColumnName, Name = "IX_" + TableName + "_Name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the serialized value representing the schema of the created package.
    /// </summary>
    [Column("value")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Value { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time when the package schema was last updated.
    /// </summary>
    [Column("updateDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the package.
    /// </summary>
    [Column("packageId")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public Guid PackageId { get; set; }
}
