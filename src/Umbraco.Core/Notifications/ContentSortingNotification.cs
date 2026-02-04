// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Called while content is sorting, but before content has been sorted. Cancel the operation to cancel the sorting.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the sort operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public sealed class ContentSortingNotification : SortingNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSortingNotification"/> class.
    /// </summary>
    /// <param name="target">The content items being sorted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentSortingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
