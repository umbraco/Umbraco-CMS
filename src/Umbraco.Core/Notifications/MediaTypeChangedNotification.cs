using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a MediaType is saved or deleted, after the transaction has completed. This is mainly used for caching purposes, and generally not recommended. Use <see cref="MediaTypeSavedNotification"/> and <see cref="MediaTypeDeletedNotification"/> instead.
/// </summary>
public class MediaTypeChangedNotification : ContentTypeChangeNotification<IMediaType>
{
    public MediaTypeChangedNotification(ContentTypeChange<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaTypeChangedNotification(IEnumerable<ContentTypeChange<IMediaType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
