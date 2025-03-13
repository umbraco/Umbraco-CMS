// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// Notification that is send out when a Content item has been scaffolded from an original item and basic cleaning has been performed
/// </summary>
public sealed class ContentScaffoldedNotification : ScaffoldedNotification<IContent>
{
    public ContentScaffoldedNotification(IContent original, IContent scaffold, int parentId, EventMessages messages)
        : base(original, scaffold, parentId, messages)
    {
    }
}
