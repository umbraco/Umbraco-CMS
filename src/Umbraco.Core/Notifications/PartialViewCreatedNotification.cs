// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after a partial view has been created.
/// </summary>
/// <remarks>
///     This notification is published after a partial view file has been successfully created
///     in the file system.
/// </remarks>
public class PartialViewCreatedNotification : CreatedNotification<IPartialView>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewCreatedNotification"/> class.
    /// </summary>
    /// <param name="target">The partial view that was created.</param>
    /// <param name="messages">The event messages collection.</param>
    public PartialViewCreatedNotification(IPartialView target, EventMessages messages)
        : base(target, messages)
    {
    }
}
