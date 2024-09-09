// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ILocalizationService when the Delete (IDictionaryItem overload) method is called in the API, after the dictionary items has been deleted.
/// </summary>
public class DictionaryItemDeletedNotification : DeletedNotification<IDictionaryItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryItemDeletedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of the deleted IDictionaryItem objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public DictionaryItemDeletedNotification(IDictionaryItem target, EventMessages messages)
        : base(target, messages)
    {
    }
}
