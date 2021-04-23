// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class PartialViewSavedNotification : SavedNotification<IPartialView>
    {
        public PartialViewSavedNotification(IPartialView target, EventMessages messages) : base(target, messages)
        {
        }

        public PartialViewSavedNotification(IEnumerable<IPartialView> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
