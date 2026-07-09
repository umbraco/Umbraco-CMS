// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when a member needs to be refreshed in the cache.
/// </summary>
/// <remarks>
///     This notification is used internally for cache synchronization purposes.
///     For normal member operations, use tree change notifications instead.
/// </remarks>
[Obsolete("This is only used for the internal cache and will change, use tree change notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class MemberRefreshNotification : EntityRefreshNotification<IMember>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberRefreshNotification"/> class.
    /// </summary>
    /// <param name="target">The member to refresh.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberRefreshNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }
}
