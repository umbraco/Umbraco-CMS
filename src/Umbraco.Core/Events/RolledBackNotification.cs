// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Events
{
    public abstract class RolledBackNotification<T> : ObjectNotification<T> where T : class
    {
        protected RolledBackNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T Entity => Target;
    }
}
