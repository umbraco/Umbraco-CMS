// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class ContentEmptiedRecycleBinNotification : EmptiedRecycleBinNotification<IContent>
    {
        public ContentEmptiedRecycleBinNotification(EventMessages messages) : base(messages)
        {
        }
    }
}
