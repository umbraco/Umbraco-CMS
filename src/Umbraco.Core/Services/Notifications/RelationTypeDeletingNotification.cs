// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class RelationTypeDeletingNotification : DeletingNotification<IRelationType>
    {
        public RelationTypeDeletingNotification(IRelationType target, EventMessages messages) : base(target, messages)
        {
        }

        public RelationTypeDeletingNotification(IEnumerable<IRelationType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
