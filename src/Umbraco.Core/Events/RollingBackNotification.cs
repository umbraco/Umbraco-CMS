// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Events
{
    public abstract class RollingBackNotification<T> : CancelableObjectNotification<T> where T : class
    {
        protected RollingBackNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T Entity => Target;
    }
}
