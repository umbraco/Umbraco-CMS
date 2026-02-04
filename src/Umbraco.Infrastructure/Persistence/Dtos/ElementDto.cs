using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(IPublishableContentDto<ElementVersionDto>.Columns.NodeId, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ElementDto : IPublishableContentDto<ElementVersionDto>
{
    internal const string TableName = Constants.DatabaseSchema.Tables.Element;

    [Column(IPublishableContentDto<ElementVersionDto>.Columns.NodeId)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentDto))]
    public int NodeId { get; set; }

    [Column(IPublishableContentDto<ElementVersionDto>.Columns.Published)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Published")]
    public bool Published { get; set; }

    [Column("edited")]
    public bool Edited { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
    public ContentDto ContentDto { get; set; } = null!;

    // although a content has many content versions,
    // they can only be loaded one by one (as several content),
    // so this here is a OneToOne reference
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ElementVersionDto ContentVersionDto { get; set; } = null!;

    // same
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ElementVersionDto? PublishedVersionDto { get; set; }
}
