// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IContentService when the Move method is called in the API.
/// Called while content is moving, but before it has been moved. Cancel the operation to prevent the movement.
/// NOTE: If the target parent is the Recycle bin, this notification is never published. Try the <see cref="ContentMovingToRecycleBinNotification"/> instead.
/// </summary>
public sealed class ContentMovingNotification : MovingNotification<IContent>
{
    public ContentMovingNotification(MoveEventInfo<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentMovingNotification(IEnumerable<MoveEventInfo<IContent>> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
