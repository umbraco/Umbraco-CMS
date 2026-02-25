using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class ContentDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Content;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string ContentTypeIdColumnName = "contentTypeId";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    [Column(ContentTypeIdColumnName)]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    public int ContentTypeId { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = PrimaryKeyColumnName)]
    public NodeDto NodeDto { get; set; } = null!;

    // although a content has many content versions,
    // they can only be loaded one by one (as several content),
    // so this here is a OneToOne reference
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = nameof(ContentVersionDto.NodeId))]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
