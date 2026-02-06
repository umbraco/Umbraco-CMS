// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before a partial view is created.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the create operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class PartialViewCreatingNotification : CreatingNotification<IPartialView>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewCreatingNotification"/> class.
    /// </summary>
    /// <param name="target">The partial view being created.</param>
    /// <param name="messages">The event messages collection.</param>
    public PartialViewCreatingNotification(IPartialView target, EventMessages messages)
        : base(target, messages)
    {
    }
}
