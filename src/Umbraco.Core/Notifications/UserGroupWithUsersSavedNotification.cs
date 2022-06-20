// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public sealed class UserGroupWithUsersSavedNotification : SavedNotification<UserGroupWithUsers>
{
    public UserGroupWithUsersSavedNotification(UserGroupWithUsers target, EventMessages messages)
        : base(target, messages)
    {
    }

    public UserGroupWithUsersSavedNotification(IEnumerable<UserGroupWithUsers> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
