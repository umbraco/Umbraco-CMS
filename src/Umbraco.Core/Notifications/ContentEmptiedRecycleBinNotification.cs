// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentEmptiedRecycleBinNotification : EmptiedRecycleBinNotification<IContent>
{
    public ContentEmptiedRecycleBinNotification(IEnumerable<IContent> deletedEntities, EventMessages messages)
        : base(
        deletedEntities, messages)
    {
    }
}
