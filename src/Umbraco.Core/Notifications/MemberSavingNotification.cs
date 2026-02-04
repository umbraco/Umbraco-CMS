// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a member is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IMemberService"/> before the member is persisted.
/// </remarks>
public sealed class MemberSavingNotification : SavingNotification<IMember>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberSavingNotification"/> class with a single member.
    /// </summary>
    /// <param name="target">The member being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberSavingNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberSavingNotification"/> class with multiple members.
    /// </summary>
    /// <param name="target">The collection of members being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberSavingNotification(IEnumerable<IMember> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
