// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class EmptiedRecycleBinNotification<T> : StatefulNotification
    where T : class
{
    protected EmptiedRecycleBinNotification(IEnumerable<T> deletedEntities, EventMessages messages)
    {
        DeletedEntities = deletedEntities;
        Messages = messages;
    }

    public IEnumerable<T> DeletedEntities { get; }

    public EventMessages Messages { get; }
}
