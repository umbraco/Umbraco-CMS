// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the SendToPublication method is called in the API, after the entity has been sent to publication.
/// </summary>
public sealed class ContentSentToPublishNotification : ObjectNotification<IContent>
{
    public ContentSentToPublishNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Getting the IContent object being sent to publish.
    /// </summary>
    public IContent Entity => Target;
}
