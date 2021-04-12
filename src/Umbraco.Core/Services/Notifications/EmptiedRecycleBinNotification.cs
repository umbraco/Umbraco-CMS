// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public abstract class EmptiedRecycleBinNotification<T> : StatefulNotification where T : class
    {
        protected EmptiedRecycleBinNotification(EventMessages messages) => Messages = messages;

        public EventMessages Messages { get; }
    }
}
