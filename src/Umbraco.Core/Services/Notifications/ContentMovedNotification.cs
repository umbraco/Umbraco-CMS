// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class ContentMovedNotification : MovedNotification<IContent>
    {
        public ContentMovedNotification(MoveEventInfo<IContent> target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentMovedNotification(IEnumerable<MoveEventInfo<IContent>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
