using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.Models;

/// <summary>
///     Snapshot of the <see cref="PropertyTypeDto" /> as it was at version 8.0
/// </summary>
/// <remarks>
///     This is required during migrations before 8.6 since the schema has changed and running SQL against the new table
///     would result in errors
/// </remarks>
[TableName(Constants.DatabaseSchema.Tables.PropertyType)]
[PrimaryKey("id")]
[ExplicitColumns]
internal class PropertyTypeDto80
{
    [Column("id")]
    [PrimaryKeyColumn(IdentitySeed = 50)]
    public int Id { get; set; }

    [Column("dataTypeId")]
    [ForeignKey(typeof(DataTypeDto), Column = "nodeId")]
    public int DataTypeId { get; set; }

    [Column("contentTypeId")]
    [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
    public int ContentTypeId { get; set; }

    [Column("propertyTypeGroupId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(PropertyTypeGroupDto))]
    public int? PropertyTypeGroupId { get; set; }

    [Index(IndexTypes.NonClustered, Name = "IX_cmsPropertyTypeAlias")]
    [Column("Alias")]
    public string Alias { get; set; } = null!;

    [Column("Name")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }

    [Column("sortOrder")]
    [Constraint(Default = "0")]
    public int SortOrder { get; set; }

    [Column("mandatory")]
    [Constraint(Default = "0")]
    public bool Mandatory { get; set; }

    [Column("validationRegExp")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? ValidationRegExp { get; set; }

    [Column("Description")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(2000)]
    public string? Description { get; set; }

    [Column("variations")]
    [Constraint(Default = "1" /*ContentVariation.InvariantNeutral*/)]
    public byte Variations { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = "DataTypeId")]
    public DataTypeDto? DataTypeDto { get; set; }

    [Column("UniqueID")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsPropertyTypeUniqueID")]
    public Guid UniqueId { get; set; }
}
