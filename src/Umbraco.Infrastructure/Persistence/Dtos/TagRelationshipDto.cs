using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey([NodeIdColumnName, PropertyTypeIdColumnName, TagIdColumnName], AutoIncrement = false)]
[ExplicitColumns]
internal sealed class TagRelationshipDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.TagRelationship;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string PropertyTypeIdColumnName = "propertyTypeId";
    public const string TagIdColumnName = "tagId";

    /// <summary>
    /// Gets or sets the node identifier associated with the tag relationship.
    /// </summary>
    [Column(NodeIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsTagRelationship", OnColumns = $"{NodeIdColumnName}, {PropertyTypeIdColumnName}, {TagIdColumnName}")]
    [ForeignKey(typeof(ContentDto), Name = "FK_cmsTagRelationship_cmsContent", Column = ContentDto.PrimaryKeyColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_tagId_nodeId", ForColumns = $"{TagIdColumnName},{NodeIdColumnName}", IncludeColumns = PropertyTypeIdColumnName)]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the tag.
    /// </summary>
    [Column(TagIdColumnName)]
    [ForeignKey(typeof(TagDto))]
    public int TagId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the property type associated with this tag relationship.
    /// </summary>
    [Column(PropertyTypeIdColumnName)]
    [ForeignKey(typeof(PropertyTypeDto), Name = "FK_cmsTagRelationship_cmsPropertyType")]
    public int PropertyTypeId { get; set; }
}
