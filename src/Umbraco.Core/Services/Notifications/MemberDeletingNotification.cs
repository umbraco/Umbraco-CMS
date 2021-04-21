// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class MemberDeletingNotification : DeletingNotification<IMember>
    {
        public MemberDeletingNotification(IMember target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberDeletingNotification(IEnumerable<IMember> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
