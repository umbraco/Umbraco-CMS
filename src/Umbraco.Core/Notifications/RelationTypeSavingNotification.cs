// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class RelationTypeSavingNotification : SavingNotification<IRelationType>
{
    public RelationTypeSavingNotification(IRelationType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public RelationTypeSavingNotification(IEnumerable<IRelationType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
