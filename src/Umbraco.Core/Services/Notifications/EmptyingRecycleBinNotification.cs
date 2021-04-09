// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public abstract class EmptyingRecycleBinNotification<T> : StatefulNotification, ICancelableNotification where T : class
    {
        protected EmptyingRecycleBinNotification(EventMessages messages) => Messages = messages;

        public EventMessages Messages { get; }

        public bool Cancel { get; set; }
    }
}
