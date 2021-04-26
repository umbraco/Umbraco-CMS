// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class DomainDeletedNotification : DeletedNotification<IDomain>
    {
        public DomainDeletedNotification(IDomain target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
