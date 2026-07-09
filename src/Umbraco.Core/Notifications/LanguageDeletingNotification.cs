// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ILocalizationService when the Delete (ILanguage overload) method is called in the API.
/// </summary>
public class LanguageDeletingNotification : DeletingNotification<ILanguage>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageDeletingNotification"/> class
    ///     with a single language.
    /// </summary>
    /// <param name="target">The language being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public LanguageDeletingNotification(ILanguage target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageDeletingNotification"/> class
    ///     with multiple languages.
    /// </summary>
    /// <param name="target">The languages being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public LanguageDeletingNotification(IEnumerable<ILanguage> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
