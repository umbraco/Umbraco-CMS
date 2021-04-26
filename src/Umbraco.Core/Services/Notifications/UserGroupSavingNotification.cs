// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class UserGroupSavingNotification : SavingNotification<IUserGroup>
    {
        public UserGroupSavingNotification(IUserGroup target, EventMessages messages) : base(target, messages)
        {
        }

        public UserGroupSavingNotification(IEnumerable<IUserGroup> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
