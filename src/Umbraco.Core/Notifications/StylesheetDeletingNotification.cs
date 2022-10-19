// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class StylesheetDeletingNotification : DeletingNotification<IStylesheet>
{
    public StylesheetDeletingNotification(IStylesheet target, EventMessages messages)
        : base(target, messages)
    {
    }

    public StylesheetDeletingNotification(IEnumerable<IStylesheet> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
