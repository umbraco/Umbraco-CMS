// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class ContentSavedNotification : SavedNotification<IContent>
    {
        public ContentSavedNotification(IContent target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentSavedNotification(IEnumerable<IContent> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
