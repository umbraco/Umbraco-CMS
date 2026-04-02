// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after entity containers (folders) have been deleted.
/// </summary>
/// <remarks>
///     Entity containers are used to organize content types, media types, and data types
///     into folders. This notification is published after a container has been deleted.
/// </remarks>
public class EntityContainerDeletedNotification : DeletedNotification<EntityContainer>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityContainerDeletedNotification"/> class.
    /// </summary>
    /// <param name="target">The entity container that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public EntityContainerDeletedNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
