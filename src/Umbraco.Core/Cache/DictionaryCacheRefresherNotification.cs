﻿using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    public class DictionaryCacheRefresherNotification : CacheRefresherNotification
    {
        public DictionaryCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
