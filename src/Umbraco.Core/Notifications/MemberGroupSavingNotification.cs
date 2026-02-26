// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before member groups are saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class MemberGroupSavingNotification : SavingNotification<IMemberGroup>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupSavingNotification"/> class
    ///     with a single member group.
    /// </summary>
    /// <param name="target">The member group being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberGroupSavingNotification(IMemberGroup target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupSavingNotification"/> class
    ///     with multiple member groups.
    /// </summary>
    /// <param name="target">The member groups being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberGroupSavingNotification(IEnumerable<IMemberGroup> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
