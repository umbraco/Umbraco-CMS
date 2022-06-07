// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class PublicAccessEntryDeletingNotification : DeletingNotification<PublicAccessEntry>
{
    public PublicAccessEntryDeletingNotification(PublicAccessEntry target, EventMessages messages)
        : base(target, messages)
    {
    }

    public PublicAccessEntryDeletingNotification(IEnumerable<PublicAccessEntry> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
