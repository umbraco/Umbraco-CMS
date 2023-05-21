// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// Called while content is sorting, but before content has been sorted. Cancel the operation to cancel the sorting.
/// </summary>
public sealed class ContentSortingNotification : SortingNotification<IContent>
{
    public ContentSortingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
