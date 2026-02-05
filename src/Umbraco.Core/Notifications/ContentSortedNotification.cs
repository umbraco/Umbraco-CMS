// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Called after content has been sorted.
/// </summary>
public sealed class ContentSortedNotification : SortedNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSortedNotification"/> class.
    /// </summary>
    /// <param name="target">The content items that were sorted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentSortedNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
