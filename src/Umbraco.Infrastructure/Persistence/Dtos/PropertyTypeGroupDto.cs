using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = true)]
[ExplicitColumns]
internal class PropertyTypeGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyTypeGroup;

    [Column("id")]
    [PrimaryKeyColumn(IdentitySeed = 56)]
    public int Id { get; set; }

    [Column("uniqueID")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsPropertyTypeGroupUniqueID")]
    public Guid UniqueId { get; set; }

    [Column("contenttypeNodeId")]
    [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
    public int ContentTypeNodeId { get; set; }

    [Column("type")]
    [Constraint(Default = 0)]
    public short Type { get; set; }

    [Column("text")]
    public string? Text { get; set; }

    [Column("alias")]
    public string Alias { get; set; } = null!;

    [Column("sortorder")]
    public int SortOrder { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = "PropertyTypeGroupId")]
    public List<PropertyTypeDto>? PropertyTypeDtos { get; set; }
}
