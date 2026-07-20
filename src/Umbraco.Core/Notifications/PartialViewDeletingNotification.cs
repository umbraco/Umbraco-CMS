// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before a partial view is deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class PartialViewDeletingNotification : DeletingNotification<IPartialView>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewDeletingNotification"/> class
    ///     with a single partial view.
    /// </summary>
    /// <param name="target">The partial view being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public PartialViewDeletingNotification(IPartialView target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewDeletingNotification"/> class
    ///     with multiple partial views.
    /// </summary>
    /// <param name="target">The partial views being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public PartialViewDeletingNotification(IEnumerable<IPartialView> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
