// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class RelationSavingNotification : SavingNotification<IRelation>
    {
        public RelationSavingNotification(IRelation target, EventMessages messages) : base(target, messages)
        {
        }

        public RelationSavingNotification(IEnumerable<IRelation> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
