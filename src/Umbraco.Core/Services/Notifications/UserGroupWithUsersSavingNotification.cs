// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class UserGroupWithUsersSavingNotification : SavingNotification<UserGroupWithUsers>
    {
        public UserGroupWithUsersSavingNotification(UserGroupWithUsers target, EventMessages messages) : base(target, messages)
        {
        }

        public UserGroupWithUsersSavingNotification(IEnumerable<UserGroupWithUsers> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
