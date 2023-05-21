// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class RelationTypeDeletedNotification : DeletedNotification<IRelationType>
{
    public RelationTypeDeletedNotification(IRelationType target, EventMessages messages)
        : base(target, messages)
    {
    }
}
