// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after an external member has been deleted.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IExternalMemberService"/> after the external member has been removed.
///     It is not cancelable since the delete operation has already completed.
/// </remarks>
public sealed class ExternalMemberDeletedNotification : DeletedNotification<ExternalMemberIdentity>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberDeletedNotification"/> class with a single external member.
    /// </summary>
    /// <param name="target">The external member that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ExternalMemberDeletedNotification(ExternalMemberIdentity target, EventMessages messages)
        : base(target, messages)
    {
    }
}
