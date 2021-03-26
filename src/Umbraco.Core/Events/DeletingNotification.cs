// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public abstract class DeletingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        protected DeletingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        protected DeletingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> DeletedEntities => Target;
    }
}
