// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public abstract class SavedNotification<T> : EnumerableObjectNotification<T>
    {
        protected SavedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        protected SavedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SavedEntities => Target;
    }
}
