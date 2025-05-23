// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IContentService when the Publishing method is called in the API.
/// Called while publishing content but before content has been published. Cancel the operation to prevent the publish.
/// </summary>
public sealed class ContentPublishingNotification : CancelableEnumerableObjectNotification<IContent>
{
    public ContentPublishingNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentPublishingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    /// Gets a enumeration of <see cref="IContent"/> which are being published.
    /// </summary>
    public IEnumerable<IContent> PublishedEntities => Target;
}
