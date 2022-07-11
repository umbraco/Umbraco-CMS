using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("nodeId", AutoIncrement = false)]
[ExplicitColumns]
internal class TagRelationshipDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.TagRelationship;

    [Column("nodeId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsTagRelationship", OnColumns = "nodeId, propertyTypeId, tagId")]
    [ForeignKey(typeof(ContentDto), Name = "FK_cmsTagRelationship_cmsContent", Column = "nodeId")]
    public int NodeId { get; set; }

    [Column("tagId")]
    [ForeignKey(typeof(TagDto))]
    public int TagId { get; set; }

    [Column("propertyTypeId")]
    [ForeignKey(typeof(PropertyTypeDto), Name = "FK_cmsTagRelationship_cmsPropertyType")]
    public int PropertyTypeId { get; set; }
}
