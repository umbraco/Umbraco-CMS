using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// this is a special Dto that does not have a corresponding table
// and is only used in our code to represent a media item, similar
// to document items.
internal sealed class MediaDto
{
    public int NodeId { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = ContentDto.ReferenceMemberName)]
    public ContentDto ContentDto { get; set; } = null!;

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public MediaVersionDto MediaVersionDto { get; set; } = null!;
}
