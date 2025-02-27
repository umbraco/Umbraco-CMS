// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ILocalizationService when the Save (ILanguage overload) method is called in the API, after data has been persisted.
/// </summary>
public class LanguageSavedNotification : SavedNotification<ILanguage>
{
    public LanguageSavedNotification(ILanguage target, EventMessages messages)
        : base(target, messages)
    {
    }

    public LanguageSavedNotification(IEnumerable<ILanguage> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
