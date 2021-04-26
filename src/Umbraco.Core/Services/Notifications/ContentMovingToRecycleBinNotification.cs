// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class ContentMovingToRecycleBinNotification : MovingToRecycleBinNotification<IContent>
    {
        public ContentMovingToRecycleBinNotification(MoveEventInfo<IContent> target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentMovingToRecycleBinNotification(IEnumerable<MoveEventInfo<IContent>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
