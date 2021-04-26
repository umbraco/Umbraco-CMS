// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class StylesheetDeletedNotification : DeletedNotification<IStylesheet>
    {
        public StylesheetDeletedNotification(IStylesheet target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
