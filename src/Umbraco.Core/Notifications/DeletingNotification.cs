// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications
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
