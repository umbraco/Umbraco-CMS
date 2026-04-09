// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when an entity needs to be refreshed in the cache.
/// </summary>
/// <typeparam name="T">The type of content base entity being refreshed.</typeparam>
/// <remarks>
///     This notification is used internally for cache synchronization purposes.
/// </remarks>
public class EntityRefreshNotification<T> : ObjectNotification<T>
    where T : class, IContentBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityRefreshNotification{T}"/> class.
    /// </summary>
    /// <param name="target">The entity to refresh.</param>
    /// <param name="messages">The event messages collection.</param>
    public EntityRefreshNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the entity that needs to be refreshed.
    /// </summary>
    public T Entity => Target;
}
