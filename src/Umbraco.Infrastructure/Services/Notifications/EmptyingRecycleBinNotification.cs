// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class EmptyingRecycleBinNotification<T> : StatefulNotification, ICancelableNotification where T : class
    {
        public EmptyingRecycleBinNotification(EventMessages messages) => Messages = messages;

        public EventMessages Messages { get; }

        public bool Cancel { get; set; }
    }
}
