// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class DomainSavedNotification : SavedNotification<IDomain>
    {
        public DomainSavedNotification(IDomain target, EventMessages messages) : base(target, messages)
        {
        }

        public DomainSavedNotification(IEnumerable<IDomain> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
