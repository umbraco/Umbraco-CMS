// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class UnpublishedNotification<T> : EnumerableObjectNotification<T>
    {
        public UnpublishedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public UnpublishedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> UnpublishedEntities => Target;
    }
}
