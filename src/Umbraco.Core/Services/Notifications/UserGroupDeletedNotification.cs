// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class UserGroupDeletedNotification : DeletedNotification<IUserGroup>
    {
        public UserGroupDeletedNotification(IUserGroup target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
