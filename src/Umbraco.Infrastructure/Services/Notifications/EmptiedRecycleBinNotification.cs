// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class EmptiedRecycleBinNotification<T> : StatefulNotification where T : class
    {
        public EmptiedRecycleBinNotification(EventMessages messages) => Messages = messages;

        public EventMessages Messages { get; }
    }
}
