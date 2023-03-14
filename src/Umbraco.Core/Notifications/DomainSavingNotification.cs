// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class DomainSavingNotification : SavingNotification<IDomain>
{
    public DomainSavingNotification(IDomain target, EventMessages messages)
        : base(target, messages)
    {
    }

    public DomainSavingNotification(IEnumerable<IDomain> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
