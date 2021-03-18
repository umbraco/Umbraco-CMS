// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public abstract class SortingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        protected SortingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SortedEntities => Target;
    }
}
