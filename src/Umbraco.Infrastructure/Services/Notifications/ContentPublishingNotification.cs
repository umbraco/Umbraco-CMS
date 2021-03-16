// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class ContentPublishingNotification : CancelableEnumerableObjectNotification<IContent>
    {
        public ContentPublishingNotification(IContent target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentPublishingNotification(IEnumerable<IContent> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<IContent> PublishedEntities => Target;
    }
}
