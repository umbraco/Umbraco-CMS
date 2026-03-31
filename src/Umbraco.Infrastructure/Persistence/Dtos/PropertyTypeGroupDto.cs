using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
internal sealed class PropertyTypeGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyTypeGroup;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string ContentTypeNodeIdColumnName = "contenttypeNodeId";
    public const string UniqueIdColumnName = "uniqueID";

    /// <summary>
    /// Gets or sets the unique identifier for the PropertyTypeGroup.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(IdentitySeed = 56)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the property type group.
    /// </summary>
    [Column(UniqueIdColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsPropertyTypeGroupUniqueID")]
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content type node associated with this property type group.
    /// </summary>
    [Column(ContentTypeNodeIdColumnName)]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    public int ContentTypeNodeId { get; set; }

    /// <summary>
    /// Gets or sets the type identifier for the property type group, typically used to distinguish between different group categories.
    /// </summary>
    [Column("type")]
    [Constraint(Default = 0)]
    public short Type { get; set; }

    /// <summary>
    /// Gets or sets the name or label of the property type group.
    /// </summary>
    [Column("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the unique alias identifier for the property type group.
    /// </summary>
    [Column("alias")]
    public string Alias { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sort order of the property type group.
    /// </summary>
    [Column("sortorder")]
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the collection of property type DTOs that belong to this property type group.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(PropertyTypeDto.PropertyTypeGroupId))]
    public List<PropertyTypeDto> PropertyTypeDtos { get; set; } = [];
}
