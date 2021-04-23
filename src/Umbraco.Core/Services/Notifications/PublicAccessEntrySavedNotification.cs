// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class PublicAccessEntrySavedNotification : SavedNotification<PublicAccessEntry>
    {
        public PublicAccessEntrySavedNotification(PublicAccessEntry target, EventMessages messages) : base(target, messages)
        {
        }

        public PublicAccessEntrySavedNotification(IEnumerable<PublicAccessEntry> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
