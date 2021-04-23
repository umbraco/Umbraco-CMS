// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class DictionaryItemSavingNotification : SavingNotification<IDictionaryItem>
    {
        public DictionaryItemSavingNotification(IDictionaryItem target, EventMessages messages) : base(target, messages)
        {
        }

        public DictionaryItemSavingNotification(IEnumerable<IDictionaryItem> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
