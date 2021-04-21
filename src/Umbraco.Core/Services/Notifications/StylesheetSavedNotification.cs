// Copyright (c) Umbraco.
// See LICENSE for more details

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class StylesheetSavedNotification : SavedNotification<IStylesheet>
    {
        public StylesheetSavedNotification(IStylesheet target, EventMessages messages) : base(target, messages)
        {
        }

        public StylesheetSavedNotification(IEnumerable<IStylesheet> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
