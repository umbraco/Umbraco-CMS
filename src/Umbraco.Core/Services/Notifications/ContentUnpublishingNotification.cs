// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class ContentUnpublishingNotification : CancelableEnumerableObjectNotification<IContent>
    {
        public ContentUnpublishingNotification(IContent target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentUnpublishingNotification(IEnumerable<IContent> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<IContent> UnpublishedEntities => Target;
    }
}
