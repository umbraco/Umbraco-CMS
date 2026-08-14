using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) for an element within the Umbraco CMS persistence layer.
/// </summary>
[TableName(TableName)]
[PrimaryKey(IPublishableContentDto<ElementVersionDto>.Columns.NodeId, AutoIncrement = false)]
[ExplicitColumns]
public sealed class ElementDto : IPublishableContentDto<ElementVersionDto>
{
    internal const string TableName = Constants.DatabaseSchema.Tables.Element;

    /// <summary>
    /// Gets or sets the unique identifier for the node associated with this element.
    /// </summary>
    [Column(IPublishableContentDto<ElementVersionDto>.Columns.NodeId)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentDto))]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the element is published.
    /// </summary>
    [Column(IPublishableContentDto<ElementVersionDto>.Columns.Published)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Published")]
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this element has been modified since it was last published or saved.
    /// </summary>
    [Column(IPublishableContentDto<ElementVersionDto>.Columns.Edited)]
    public bool Edited { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ContentDto"/> instance that contains the content data associated with this element.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
    public ContentDto ContentDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the element version DTO referenced in a one-to-one relationship with this element.
    /// Although an element can have multiple versions, this property represents the specific version associated with this instance.
    /// </summary>
    /// <remarks>
    /// Although a content has many content versions,
    /// they can only be loaded one by one (as several content),
    /// so this here is a OneToOne reference.
    /// </remarks>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ElementVersionDto ContentVersionDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DTO containing information about the published version of the element.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ElementVersionDto? PublishedVersionDto { get; set; }
}
