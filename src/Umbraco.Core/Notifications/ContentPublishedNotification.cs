// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IContentService when the Publish method is called in the API and after data has been published.
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
    /// <summary>
    /// Gets a enumeration of <see cref="IContent"/> which are being published.
    /// </summary>
    public IEnumerable<IContent> PublishedEntities => Target;

    public bool IncludeDescendants { get; }
}
