// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class MediaDeletedNotification : DeletedNotification<IMedia>
{
    public MediaDeletedNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }
}
