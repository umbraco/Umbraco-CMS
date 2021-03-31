// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class RelationDeletedNotification : DeletedNotification<IRelation>
    {
        public RelationDeletedNotification(IRelation target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
