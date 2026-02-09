// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a member type has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IMemberTypeService"/> after the member type has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public class MemberTypeSavedNotification : SavedNotification<IMemberType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeSavedNotification"/> class with a single member type.
    /// </summary>
    /// <param name="target">The member type that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeSavedNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeSavedNotification"/> class with multiple member types.
    /// </summary>
    /// <param name="target">The collection of member types that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeSavedNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
