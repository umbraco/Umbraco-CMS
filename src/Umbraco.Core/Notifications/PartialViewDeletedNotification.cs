// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after a partial view has been deleted.
/// </summary>
/// <remarks>
///     This notification is published after a partial view file has been successfully deleted
///     from the file system.
/// </remarks>
public class PartialViewDeletedNotification : DeletedNotification<IPartialView>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewDeletedNotification"/> class.
    /// </summary>
    /// <param name="target">The partial view that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public PartialViewDeletedNotification(IPartialView target, EventMessages messages)
        : base(target, messages)
    {
    }
}
