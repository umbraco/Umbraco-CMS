// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

public sealed class UserGroupSavedNotification : SavedNotification<IUserGroup>
{
    public UserGroupSavedNotification(IUserGroup target, EventMessages messages)
        : base(target, messages)
    {
    }

    public UserGroupSavedNotification(IEnumerable<IUserGroup> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
