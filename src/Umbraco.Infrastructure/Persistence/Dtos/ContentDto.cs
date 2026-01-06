using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
public class ContentDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Content;
    public const string PrimaryKeyName = Constants.DatabaseSchema.NodeIdName;
    public const string ContentTypeIdName = "contentTypeId";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    [Column(ContentTypeIdName)]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdName)]
    public int ContentTypeId { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = PrimaryKeyName)]
    public NodeDto NodeDto { get; set; } = null!;

    // although a content has many content versions,
    // they can only be loaded one by one (as several content),
    // so this here is a OneToOne reference
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = PrimaryKeyName)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
