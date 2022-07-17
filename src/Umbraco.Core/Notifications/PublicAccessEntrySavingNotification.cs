// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class PublicAccessEntrySavingNotification : SavingNotification<PublicAccessEntry>
{
    public PublicAccessEntrySavingNotification(PublicAccessEntry target, EventMessages messages)
        : base(target, messages)
    {
    }

    public PublicAccessEntrySavingNotification(IEnumerable<PublicAccessEntry> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
