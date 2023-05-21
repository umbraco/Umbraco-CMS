// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class MediaEmptiedRecycleBinNotification : EmptiedRecycleBinNotification<IMedia>
{
    public MediaEmptiedRecycleBinNotification(IEnumerable<IMedia> deletedEntities, EventMessages messages)
        : base(deletedEntities, messages)
    {
    }
}
