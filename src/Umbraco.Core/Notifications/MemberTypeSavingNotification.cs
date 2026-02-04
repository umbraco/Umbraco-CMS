// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a member type is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IMemberTypeService"/> before the member type is persisted.
/// </remarks>
public class MemberTypeSavingNotification : SavingNotification<IMemberType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeSavingNotification"/> class with a single member type.
    /// </summary>
    /// <param name="target">The member type being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeSavingNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeSavingNotification"/> class with multiple member types.
    /// </summary>
    /// <param name="target">The collection of member types being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeSavingNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
