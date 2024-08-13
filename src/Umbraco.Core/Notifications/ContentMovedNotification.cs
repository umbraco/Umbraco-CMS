// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IContentService when the Move method is called in the API.
/// The notification is published and called after the content object has been moved.
/// NOTE: If the target parent is the Recycle bin, this notification is never published. Try the <see cref="ContentMovedToRecycleBinNotification"/> instead.
/// </summary>
public sealed class ContentMovedNotification : MovedNotification<IContent>
{
    public ContentMovedNotification(MoveEventInfo<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentMovedNotification(IEnumerable<MoveEventInfo<IContent>> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
