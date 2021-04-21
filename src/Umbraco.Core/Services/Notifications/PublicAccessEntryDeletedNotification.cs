// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class PublicAccessEntryDeletedNotification : DeletedNotification<PublicAccessEntry>
    {
        public PublicAccessEntryDeletedNotification(PublicAccessEntry target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
