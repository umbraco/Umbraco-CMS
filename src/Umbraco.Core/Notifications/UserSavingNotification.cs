// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

public sealed class UserSavingNotification : SavingNotification<IUser>
{
    public UserSavingNotification(IUser target, EventMessages messages)
        : base(target, messages)
    {
    }

    public UserSavingNotification(IEnumerable<IUser> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
