// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class MediaDeletingNotification : DeletingNotification<IMedia>
{
    public MediaDeletingNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaDeletingNotification(IEnumerable<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
