// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public abstract class EmptyingRecycleBinNotification<T> : StatefulNotification, ICancelableNotification where T : class
    {
        protected EmptyingRecycleBinNotification(IEnumerable<T> deletedEntities, EventMessages messages)
        {
            DeletedEntities = deletedEntities;
            Messages = messages;
        }

        public IEnumerable<T> DeletedEntities { get; }

        public EventMessages Messages { get; }

        public bool Cancel { get; set; }
    }
}
