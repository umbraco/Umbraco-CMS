// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class EmptiedRecycleBinNotification<T> : INotification where T : class
    {
        public EmptiedRecycleBinNotification(EventMessages messages)
        {
            Messages = messages;
        }

        public EventMessages Messages { get; }
    }
}
