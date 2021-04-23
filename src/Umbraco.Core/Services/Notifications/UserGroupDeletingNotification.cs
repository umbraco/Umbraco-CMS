// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class UserGroupDeletingNotification : DeletingNotification<IUserGroup>
    {
        public UserGroupDeletingNotification(IUserGroup target, EventMessages messages) : base(target, messages)
        {
        }

        public UserGroupDeletingNotification(IEnumerable<IUserGroup> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
