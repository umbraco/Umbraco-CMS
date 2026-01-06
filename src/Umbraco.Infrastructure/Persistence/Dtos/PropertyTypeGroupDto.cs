using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = true)]
[ExplicitColumns]
internal sealed class PropertyTypeGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyTypeGroup;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNameId;
    public const string ContentTypeNodeIdName = "contenttypeNodeId";
    public const string UniqueIdName = "uniqueId";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(IdentitySeed = 56)]
    public int Id { get; set; }

    [Column(UniqueIdName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsPropertyTypeGroupUniqueID")]
    public Guid UniqueId { get; set; }

    [Column(ContentTypeNodeIdName)]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdName)]
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
    [Reference(ReferenceType.Many, ReferenceMemberName = PropertyTypeDto.PorpertyTypeGroupIdName)]
    public List<PropertyTypeDto> PropertyTypeDtos { get; set; } = [];
}
