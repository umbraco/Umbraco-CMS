using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a ContentType is saved or deleted, after the transaction has completed.
///  This is mainly used for caching purposes, and generally not recommended. Use <see cref="ContentTypeSavedNotification"/> and <see cref="ContentTypeDeletedNotification"/> instead.
/// </summary>
public class ContentTypeChangedNotification : ContentTypeChangeNotification<IContentType>
{
    public ContentTypeChangedNotification(ContentTypeChange<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    public ContentTypeChangedNotification(IEnumerable<ContentTypeChange<IContentType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
