using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// this is a special Dto that does not have a corresponding table
// and is only used in our code to represent a media item, similar
// to document items.
internal sealed class MediaDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the media node in the database.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ContentDto"/> instance associated with this <see cref="MediaDto"/>.
    /// Represents the underlying content data for the media item.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = nameof(ContentDto.NodeId))]
    public ContentDto ContentDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <see cref="MediaVersionDto"/> containing version information for this media entity.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public MediaVersionDto MediaVersionDto { get; set; } = null!;
}
