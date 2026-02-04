// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after entity containers (folders) have been saved.
/// </summary>
/// <remarks>
///     Entity containers are used to organize content types, media types, and data types
///     into folders. This notification is published after a container has been saved.
/// </remarks>
public class EntityContainerSavedNotification : SavedNotification<EntityContainer>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityContainerSavedNotification"/> class.
    /// </summary>
    /// <param name="target">The entity container that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public EntityContainerSavedNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
