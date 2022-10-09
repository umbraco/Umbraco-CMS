// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class LanguageDeletingNotification : DeletingNotification<ILanguage>
{
    public LanguageDeletingNotification(ILanguage target, EventMessages messages)
        : base(target, messages)
    {
    }

    public LanguageDeletingNotification(IEnumerable<ILanguage> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
