// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class RelationTypeSavedNotification : SavedNotification<IRelationType>
{
    public RelationTypeSavedNotification(IRelationType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public RelationTypeSavedNotification(IEnumerable<IRelationType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
