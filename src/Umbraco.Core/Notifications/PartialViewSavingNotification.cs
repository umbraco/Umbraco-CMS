// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before partial views are saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class PartialViewSavingNotification : SavingNotification<IPartialView>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewSavingNotification"/> class
    ///     with a single partial view.
    /// </summary>
    /// <param name="target">The partial view being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public PartialViewSavingNotification(IPartialView target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewSavingNotification"/> class
    ///     with multiple partial views.
    /// </summary>
    /// <param name="target">The partial views being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public PartialViewSavingNotification(IEnumerable<IPartialView> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
