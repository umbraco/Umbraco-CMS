﻿using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class MemberGroupSavingNotification : SavingNotification<IMemberGroup>
    {
        public MemberGroupSavingNotification(IMemberGroup target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberGroupSavingNotification(IEnumerable<IMemberGroup> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
