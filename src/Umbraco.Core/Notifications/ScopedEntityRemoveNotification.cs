// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when an entity is removed within a scope.
/// </summary>
/// <remarks>
///     This notification is used internally for cache synchronization purposes.
///     For normal entity operations, use tree change notifications instead.
/// </remarks>
[Obsolete("This is only used for the internal cache and will change, use tree change notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ScopedEntityRemoveNotification : ObjectNotification<IContentBase>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScopedEntityRemoveNotification"/> class.
    /// </summary>
    /// <param name="target">The entity being removed.</param>
    /// <param name="messages">The event messages collection.</param>
    public ScopedEntityRemoveNotification(IContentBase target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the entity being removed.
    /// </summary>
    public IContentBase Entity => Target;
}
