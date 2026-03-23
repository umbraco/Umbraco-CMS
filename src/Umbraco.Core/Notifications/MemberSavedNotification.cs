// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a member has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IMemberService"/> after the member has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public sealed class MemberSavedNotification : SavedNotification<IMember>
{
    /// <summary>
    ///     Defines the notification state key for tracking the previous username of a saved member.
    /// </summary>
    internal const string PreviousUsernameStateKey = "PreviousUsername";

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberSavedNotification"/> class with a single member.
    /// </summary>
    /// <param name="target">The member that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberSavedNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberSavedNotification"/> class with multiple members.
    /// </summary>
    /// <param name="target">The collection of members that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberSavedNotification(IEnumerable<IMember> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
