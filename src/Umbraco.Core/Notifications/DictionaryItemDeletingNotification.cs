// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ILocalizationService when the Delete (IDictionaryItem overload) method is called in the API.
/// </summary>
public class DictionaryItemDeletingNotification : DeletingNotification<IDictionaryItem>
{
    public DictionaryItemDeletingNotification(IDictionaryItem target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryItemDeletingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of deleted IDictionaryItem objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public DictionaryItemDeletingNotification(IEnumerable<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
