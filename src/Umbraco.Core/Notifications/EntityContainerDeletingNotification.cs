// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before entity containers (folders) are deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class EntityContainerDeletingNotification : DeletingNotification<EntityContainer>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityContainerDeletingNotification"/> class.
    /// </summary>
    /// <param name="target">The entity container being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public EntityContainerDeletingNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
