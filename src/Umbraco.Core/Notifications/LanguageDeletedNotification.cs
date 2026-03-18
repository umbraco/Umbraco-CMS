// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the ILocalizationService when the Delete (ILanguage overload) method is called in the API, after the languages have been deleted.
/// </summary>
public class LanguageDeletedNotification : DeletedNotification<ILanguage>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageDeletedNotification"/> class.
    /// </summary>
    /// <param name="target">The language that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public LanguageDeletedNotification(ILanguage target, EventMessages messages)
        : base(target, messages)
    {
    }
}
