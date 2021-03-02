// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class PublishingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public PublishingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public PublishingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> PublishedEntities => Target;
    }
}
