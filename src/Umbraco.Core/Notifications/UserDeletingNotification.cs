// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

public sealed class UserDeletingNotification : DeletingNotification<IUser>
{
    public UserDeletingNotification(IUser target, EventMessages messages)
        : base(target, messages)
    {
    }

    public UserDeletingNotification(IEnumerable<IUser> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
