// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class PartialViewSavingNotification : SavingNotification<IPartialView>
{
    public PartialViewSavingNotification(IPartialView target, EventMessages messages)
        : base(target, messages)
    {
    }

    public PartialViewSavingNotification(IEnumerable<IPartialView> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
