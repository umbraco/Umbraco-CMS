// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class MediaEmptyingRecycleBinNotification : EmptyingRecycleBinNotification<IMedia>
    {
        public MediaEmptyingRecycleBinNotification(EventMessages messages) : base(messages)
        {
        }
    }
}
