// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class TemplateDeletingNotification : DeletingNotification<ITemplate>
    {
        public TemplateDeletingNotification(ITemplate target, EventMessages messages) : base(target, messages)
        {
        }

        public TemplateDeletingNotification(IEnumerable<ITemplate> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
