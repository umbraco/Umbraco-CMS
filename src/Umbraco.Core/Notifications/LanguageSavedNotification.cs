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
    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageSavedNotification"/> class
    ///     with a single language.
    /// </summary>
    /// <param name="target">The language that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public LanguageSavedNotification(ILanguage target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageSavedNotification"/> class
    ///     with multiple languages.
    /// </summary>
    /// <param name="target">The languages that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public LanguageSavedNotification(IEnumerable<ILanguage> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
