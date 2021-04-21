// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class TemplateDeletedNotification : DeletedNotification<ITemplate>
    {
        public TemplateDeletedNotification(ITemplate target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
