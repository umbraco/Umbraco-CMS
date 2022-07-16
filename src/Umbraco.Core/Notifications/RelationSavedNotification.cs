// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class RelationSavedNotification : SavedNotification<IRelation>
{
    public RelationSavedNotification(IRelation target, EventMessages messages)
        : base(target, messages)
    {
    }

    public RelationSavedNotification(IEnumerable<IRelation> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
