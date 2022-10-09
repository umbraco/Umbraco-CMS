// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

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
    public ContentPublishedNotification(IEnumerable<IContent> target, EventMessages messages, bool includeDescandants) : base(target, messages)
    {
        includeDescandants = includeDescandants;
    }
    public IEnumerable<IContent> PublishedEntities => Target;
    public bool IncludeDescendants { get; set; }
}
