// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentDeletedNotification : DeletedNotification<IContent>
{
    public ContentDeletedNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
}
