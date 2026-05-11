// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ILocalizationService when the Save (ILanguage overload) method is called in the API.
/// </summary>
public class LanguageSavingNotification : SavingNotification<ILanguage>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageSavingNotification"/> class
    ///     with a single language.
    /// </summary>
    /// <param name="target">The language being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public LanguageSavingNotification(ILanguage target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageSavingNotification"/> class
    ///     with multiple languages.
    /// </summary>
    /// <param name="target">The languages being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public LanguageSavingNotification(IEnumerable<ILanguage> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
