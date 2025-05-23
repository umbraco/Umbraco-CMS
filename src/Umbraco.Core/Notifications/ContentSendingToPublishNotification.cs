// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the SendToPublication method is called in the API.
/// </summary>
public sealed class ContentSendingToPublishNotification : CancelableObjectNotification<IContent>
{
    public ContentSendingToPublishNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Getting the IContent object being sent to publish.
    /// </summary>
    public IContent Entity => Target;
}
