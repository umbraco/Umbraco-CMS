// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class UnpublishingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public UnpublishingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public UnpublishingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> UnpublishedEntities => Target;
    }
}
