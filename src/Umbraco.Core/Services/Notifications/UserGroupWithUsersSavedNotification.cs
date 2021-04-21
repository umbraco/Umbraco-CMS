// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class UserGroupWithUsersSavedNotification : SavedNotification<UserGroupWithUsers>
    {
        public UserGroupWithUsersSavedNotification(UserGroupWithUsers target, EventMessages messages) : base(target, messages)
        {
        }

        public UserGroupWithUsersSavedNotification(IEnumerable<UserGroupWithUsers> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
