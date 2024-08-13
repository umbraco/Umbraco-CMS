// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the MoveToRecycleBin method is called in the API.
/// Is published after the content has been moved to the Recycle Bin.
/// </summary>
public sealed class ContentMovedToRecycleBinNotification : MovedToRecycleBinNotification<IContent>
{
    public ContentMovedToRecycleBinNotification(MoveToRecycleBinEventInfo<IContent> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    public ContentMovedToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<IContent>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
