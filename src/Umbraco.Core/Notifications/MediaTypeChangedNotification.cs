using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a MediaType is saved or deleted, after the transaction has completed. This is mainly used for caching purposes, and generally not recommended. Use <see cref="MediaTypeSavedNotification"/> and <see cref="MediaTypeDeletedNotification"/> instead.
/// </summary>
public class MediaTypeChangedNotification : ContentTypeChangeNotification<IMediaType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeChangedNotification"/> class
    ///     with a single content type change.
    /// </summary>
    /// <param name="target">The content type change information for the media type.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTypeChangedNotification(ContentTypeChange<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeChangedNotification"/> class
    ///     with multiple content type changes.
    /// </summary>
    /// <param name="target">The content type change information for the media types.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTypeChangedNotification(IEnumerable<ContentTypeChange<IMediaType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
