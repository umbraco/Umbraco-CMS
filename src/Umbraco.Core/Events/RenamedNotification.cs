// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public abstract class RenamedNotification<T> : EnumerableObjectNotification<T>
    {
        protected RenamedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        protected RenamedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> Entities => Target;
    }
}
