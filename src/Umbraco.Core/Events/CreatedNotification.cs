// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Events
{
    public abstract class CreatedNotification<T> : ObjectNotification<T> where T : class
    {
        protected CreatedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T CreatedEntity => Target;
    }
}
