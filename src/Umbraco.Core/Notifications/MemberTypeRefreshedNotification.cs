// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when member types have been refreshed in the cache.
/// </summary>
/// <remarks>
///     This notification is used internally for cache synchronization purposes.
///     For normal member type operations, use tree change notifications instead.
/// </remarks>
[Obsolete("This is only used for the internal cache and will change, use tree change notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class MemberTypeRefreshedNotification : ContentTypeRefreshNotification<IMemberType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeRefreshedNotification"/> class
    ///     with a single member type change.
    /// </summary>
    /// <param name="target">The member type change that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeRefreshedNotification(ContentTypeChange<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeRefreshedNotification"/> class
    ///     with multiple member type changes.
    /// </summary>
    /// <param name="target">The collection of member type changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeRefreshedNotification(IEnumerable<ContentTypeChange<IMemberType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
