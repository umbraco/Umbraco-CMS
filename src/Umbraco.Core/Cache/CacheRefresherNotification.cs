using System;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// Base class for cache refresher notifications
    /// </summary>
    public abstract class CacheRefresherNotification : INotification
    {
        public CacheRefresherNotification(object messageObject, MessageType messageType)
        {
            MessageObject = messageObject ?? throw new ArgumentNullException(nameof(messageObject));
            MessageType = messageType;
        }

        public object MessageObject { get; }
        public MessageType MessageType { get; }
    }
}
