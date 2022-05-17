// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentSendingToPublishNotification : CancelableObjectNotification<IContent>
{
    public ContentSendingToPublishNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IContent Entity => Target;
}
