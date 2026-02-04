// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is sent out when a Content item has been scaffolded from an original item and basic cleaning has been performed.
/// </summary>
public sealed class ContentScaffoldedNotification : ScaffoldedNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentScaffoldedNotification"/> class.
    /// </summary>
    /// <param name="original">The original content being copied from.</param>
    /// <param name="scaffold">The scaffold (copy) being created.</param>
    /// <param name="parentId">The ID of the parent under which the scaffold will be created.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentScaffoldedNotification(IContent original, IContent scaffold, int parentId, EventMessages messages)
        : base(original, scaffold, parentId, messages)
    {
    }
}
