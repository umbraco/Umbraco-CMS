using System;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// Event args for cache refresher updates
    /// </summary>
    public class CacheRefresherEventArgs : EventArgs
    {
        public CacheRefresherEventArgs(object msgObject, MessageType type)
        {
            MessageType = type;
            MessageObject = msgObject;
        }
        public object MessageObject { get; private set; }
        public MessageType MessageType { get; private set; }
    }
}
