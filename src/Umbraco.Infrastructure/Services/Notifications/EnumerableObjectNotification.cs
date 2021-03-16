// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public abstract class EnumerableObjectNotification<T> : ObjectNotification<IEnumerable<T>>
    {
        protected EnumerableObjectNotification(T target, EventMessages messages) : base(new [] {target}, messages)
        {
        }

        protected EnumerableObjectNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
