// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class PublishedNotification<T> : EnumerableObjectNotification<T>
    {
        public PublishedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public PublishedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> PublishedEntities => Target;
    }
}
