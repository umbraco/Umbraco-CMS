// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after partial views have been saved.
/// </summary>
/// <remarks>
///     This notification is published after partial view files have been successfully saved
///     to the file system.
/// </remarks>
public class PartialViewSavedNotification : SavedNotification<IPartialView>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewSavedNotification"/> class
    ///     with a single partial view.
    /// </summary>
    /// <param name="target">The partial view that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public PartialViewSavedNotification(IPartialView target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewSavedNotification"/> class
    ///     with multiple partial views.
    /// </summary>
    /// <param name="target">The partial views that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public PartialViewSavedNotification(IEnumerable<IPartialView> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
