// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the UnPublishing method is called in the API.
/// </summary>
public sealed class ContentUnpublishingNotification : CancelableEnumerableObjectNotification<IContent>
{
    public ContentUnpublishingNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentUnpublishingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Gets a enumeration of <see cref="IContent"/> which are being unpublished.
    /// </summary>
    public IEnumerable<IContent> UnpublishedEntities => Target;
}
