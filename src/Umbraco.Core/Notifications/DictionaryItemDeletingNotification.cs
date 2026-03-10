// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the ILocalizationService when the Delete (IDictionaryItem overload) method is called in the API.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class DictionaryItemDeletingNotification : DeletingNotification<IDictionaryItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryItemDeletingNotification"/> class
    ///     with a single dictionary item.
    /// </summary>
    /// <param name="target">The dictionary item being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public DictionaryItemDeletingNotification(IDictionaryItem target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryItemDeletingNotification"/> class
    ///     with multiple dictionary items.
    /// </summary>
    /// <param name="target">The dictionary items being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public DictionaryItemDeletingNotification(IEnumerable<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
