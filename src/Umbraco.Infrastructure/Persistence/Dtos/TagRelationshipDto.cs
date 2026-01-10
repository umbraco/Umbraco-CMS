using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class TagRelationshipDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.TagRelationship;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string PropertyTypeIdColumnName = "propertyTypeId";
    public const string TagIdColumnName = "tagId";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsTagRelationship", OnColumns = $"{PrimaryKeyColumnName}, {PropertyTypeIdColumnName}, {TagIdColumnName}")]
    [ForeignKey(typeof(ContentDto), Name = "FK_cmsTagRelationship_cmsContent", Column = ContentDto.PrimaryKeyColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_tagId_nodeId", ForColumns = $"{TagIdColumnName},{PrimaryKeyColumnName}", IncludeColumns = PropertyTypeIdColumnName)]
    public int NodeId { get; set; }

    [Column(TagIdColumnName)]
    [ForeignKey(typeof(TagDto))]
    public int TagId { get; set; }

    [Column(PropertyTypeIdColumnName)]
    [ForeignKey(typeof(PropertyTypeDto), Name = "FK_cmsTagRelationship_cmsPropertyType")]
    public int PropertyTypeId { get; set; }
}
