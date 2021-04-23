// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class UserSavedNotification : SavedNotification<IUser>
    {
        public UserSavedNotification(IUser target, EventMessages messages) : base(target, messages)
        {
        }

        public UserSavedNotification(IEnumerable<IUser> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
