// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class DictionaryItemDeletingNotification : DeletingNotification<IDictionaryItem>
{
    public DictionaryItemDeletingNotification(IDictionaryItem target, EventMessages messages)
        : base(target, messages)
    {
    }

    public DictionaryItemDeletingNotification(IEnumerable<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
