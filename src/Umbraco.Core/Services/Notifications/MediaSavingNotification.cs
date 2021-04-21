// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class MediaSavingNotification : SavingNotification<IMedia>
    {
        public MediaSavingNotification(IMedia target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaSavingNotification(IEnumerable<IMedia> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
