// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after an external member has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IExternalMemberService"/> after the external member has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public sealed class ExternalMemberSavedNotification : SavedNotification<ExternalMemberIdentity>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberSavedNotification"/> class with a single external member.
    /// </summary>
    /// <param name="target">The external member that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ExternalMemberSavedNotification(ExternalMemberIdentity target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberSavedNotification"/> class with multiple external members.
    /// </summary>
    /// <param name="target">The collection of external members that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ExternalMemberSavedNotification(IEnumerable<ExternalMemberIdentity> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
