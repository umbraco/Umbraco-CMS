// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentSavingNotification : SavingNotification<IContent>
{
    public ContentSavingNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentSavingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
