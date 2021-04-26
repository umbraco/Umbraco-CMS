// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class RelationDeletedNotification : DeletedNotification<IRelation>
    {
        public RelationDeletedNotification(IRelation target, EventMessages messages) : base(target, messages)
        {
        }

        public RelationDeletedNotification(IEnumerable<IRelation> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
