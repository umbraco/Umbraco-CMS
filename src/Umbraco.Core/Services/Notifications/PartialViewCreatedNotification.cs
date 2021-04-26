// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class PartialViewCreatedNotification : CreatedNotification<IPartialView>
    {
        public PartialViewCreatedNotification(IPartialView target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
