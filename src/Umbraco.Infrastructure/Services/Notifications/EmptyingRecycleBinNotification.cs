// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class EmptyingRecycleBinNotification<T> : EmptiedRecycleBinNotification<T>, ICancelableNotification where T : class
    {
        public EmptyingRecycleBinNotification(EventMessages messages)
            : base(messages)
        {
        }

        public bool Cancel { get; set; }
    }
}
