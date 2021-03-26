// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
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
