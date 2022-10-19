// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class DictionaryItemSavedNotification : SavedNotification<IDictionaryItem>
{
    public DictionaryItemSavedNotification(IDictionaryItem target, EventMessages messages)
        : base(target, messages)
    {
    }

    public DictionaryItemSavedNotification(IEnumerable<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
