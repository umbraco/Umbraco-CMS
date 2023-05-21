// Copyright (c) Umbraco.
// See LICENSE for more details

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class StylesheetSavingNotification : SavingNotification<IStylesheet>
{
    public StylesheetSavingNotification(IStylesheet target, EventMessages messages)
        : base(target, messages)
    {
    }

    public StylesheetSavingNotification(IEnumerable<IStylesheet> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
