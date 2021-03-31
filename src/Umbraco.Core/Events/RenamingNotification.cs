// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public abstract class RenamingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        protected RenamingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        protected RenamingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> Entities => Target;
    }
}
