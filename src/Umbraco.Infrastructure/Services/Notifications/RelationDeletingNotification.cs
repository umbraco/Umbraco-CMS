// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class RelationDeletingNotification : DeletingNotification<IRelation>
    {
        public RelationDeletingNotification(IRelation target, EventMessages messages) : base(target, messages)
        {
        }

        public RelationDeletingNotification(IEnumerable<IRelation> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
