// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class DomainDeletingNotification : DeletingNotification<IDomain>
{
    public DomainDeletingNotification(IDomain target, EventMessages messages)
        : base(target, messages)
    {
    }

    public DomainDeletingNotification(IEnumerable<IDomain> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
