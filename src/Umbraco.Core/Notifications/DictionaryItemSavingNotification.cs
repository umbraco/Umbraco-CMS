// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ILocalizationService when the Save (IDictionaryItem overload) method is called in the API.
/// </summary>
public class DictionaryItemSavingNotification : SavingNotification<IDictionaryItem>
{
    /// <summary>
    /// Initializes a new instance of the  <see cref="DictionaryItemSavingNotification"/>
    /// </summary>
    /// <param name="target">
    /// Gets the collection of IDictionaryItem objects being saved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public DictionaryItemSavingNotification(IDictionaryItem target, EventMessages messages)
        : base(target, messages)
    {
    }

    public DictionaryItemSavingNotification(IEnumerable<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
