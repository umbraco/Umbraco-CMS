using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class TagRelationshipDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.TagRelationship;
    public const string PrimaryKeyName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string PropertyTypeIdName = "propertyTypeId";
    public const string TagIdName = "tagId";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsTagRelationship", OnColumns = $"{PrimaryKeyName}, {PropertyTypeIdName}, {TagIdName}")]
    [ForeignKey(typeof(ContentDto), Name = "FK_cmsTagRelationship_cmsContent", Column = ContentDto.PrimaryKeyName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_tagId_nodeId", ForColumns = $"{TagIdName},{PrimaryKeyName}", IncludeColumns = PropertyTypeIdName)]
    public int NodeId { get; set; }

    [Column(TagIdName)]
    [ForeignKey(typeof(TagDto))]
    public int TagId { get; set; }

    [Column(PropertyTypeIdName)]
    [ForeignKey(typeof(PropertyTypeDto), Name = "FK_cmsTagRelationship_cmsPropertyType")]
    public int PropertyTypeId { get; set; }
}
