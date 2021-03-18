// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Events
{
    public abstract class ObjectNotification<T> : StatefulNotification where T : class
    {
        protected ObjectNotification(T target, EventMessages messages)
        {
            Messages = messages;
            Target = target;
        }

        public EventMessages Messages { get; }

        protected T Target { get; }
    }
}
