// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before entity containers (folders) are renamed.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the rename operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class EntityContainerRenamingNotification : RenamingNotification<EntityContainer>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityContainerRenamingNotification"/> class.
    /// </summary>
    /// <param name="target">The entity container being renamed.</param>
    /// <param name="messages">The event messages collection.</param>
    public EntityContainerRenamingNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
