// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Events
{
    public abstract class CreatingNotification<T> : CancelableObjectNotification<T> where T : class
    {
        protected CreatingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T CreatedEntity => Target;
    }
}
