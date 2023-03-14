// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class LanguageSavingNotification : SavingNotification<ILanguage>
{
    public LanguageSavingNotification(ILanguage target, EventMessages messages)
        : base(target, messages)
    {
    }

    public LanguageSavingNotification(IEnumerable<ILanguage> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
