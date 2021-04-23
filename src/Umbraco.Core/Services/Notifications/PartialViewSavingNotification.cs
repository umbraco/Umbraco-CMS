// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class PartialViewSavingNotification : SavingNotification<IPartialView>
    {
        public PartialViewSavingNotification(IPartialView target, EventMessages messages) : base(target, messages)
        {
        }

        public PartialViewSavingNotification(IEnumerable<IPartialView> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
