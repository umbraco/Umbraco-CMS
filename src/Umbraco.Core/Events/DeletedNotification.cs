// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public abstract class DeletedNotification<T> : EnumerableObjectNotification<T>
    {
        protected DeletedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        protected DeletedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> DeletedEntities => Target;
    }
}
