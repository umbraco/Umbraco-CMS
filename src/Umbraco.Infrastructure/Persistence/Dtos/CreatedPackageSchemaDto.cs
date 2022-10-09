using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
[PrimaryKey("id")]
public class CreatedPackageSchemaDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.CreatedPackageSchema;

    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("name")]
    [Length(255)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = "name", Name = "IX_" + TableName + "_Name")]
    public string Name { get; set; } = null!;

    [Column("value")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Value { get; set; } = null!;

    [Column("updateDate")]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime UpdateDate { get; set; }

    [Column("packageId")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public Guid PackageId { get; set; }
}
