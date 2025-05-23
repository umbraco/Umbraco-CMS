// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ILocalizationService when the Save (IDictionaryItem overload) method is called in the API and the data has been persisted.
/// </summary>
public class DictionaryItemSavedNotification : SavedNotification<IDictionaryItem>
{
        public DictionaryItemSavedNotification(IDictionaryItem target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the  <see cref="DictionaryItemSavedNotification"/>
    /// </summary>
    /// <param name="target">
    /// Gets the saved collection of IDictionary objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public DictionaryItemSavedNotification(IEnumerable<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
