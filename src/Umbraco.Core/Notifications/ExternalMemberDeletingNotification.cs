// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before an external member is deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation.
///     The notification is published by the <see cref="Services.IExternalMemberService"/> before the external member is removed.
/// </remarks>
public sealed class ExternalMemberDeletingNotification : DeletingNotification<ExternalMemberIdentity>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberDeletingNotification"/> class with a single external member.
    /// </summary>
    /// <param name="target">The external member being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ExternalMemberDeletingNotification(ExternalMemberIdentity target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberDeletingNotification"/> class with multiple external members.
    /// </summary>
    /// <param name="target">The collection of external members being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ExternalMemberDeletingNotification(IEnumerable<ExternalMemberIdentity> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
