// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// Called after content has been published.
/// </summary>
public sealed class ContentPublishedNotification : EnumerableObjectNotification<IContent>
{
    public ContentPublishedNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentPublishedNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentPublishedNotification(IEnumerable<IContent> target, EventMessages messages, bool includeDescendants)
        : base(target, messages) => IncludeDescendants = includeDescendants;

    public IEnumerable<IContent> PublishedEntities => Target;

    public bool IncludeDescendants { get; }
}
