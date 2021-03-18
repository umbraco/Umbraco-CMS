// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public abstract class SavingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        protected SavingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        protected SavingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SavedEntities => Target;
    }
}
