// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the ILocalizationService when the Save (IDictionaryItem overload) method is called in the API and the data has been persisted.
/// </summary>
public class DictionaryItemSavedNotification : SavedNotification<IDictionaryItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryItemSavedNotification"/> class
    ///     with a single dictionary item.
    /// </summary>
    /// <param name="target">The dictionary item that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DictionaryItemSavedNotification(IDictionaryItem target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryItemSavedNotification"/> class
    ///     with multiple dictionary items.
    /// </summary>
    /// <param name="target">The dictionary items that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DictionaryItemSavedNotification(IEnumerable<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
