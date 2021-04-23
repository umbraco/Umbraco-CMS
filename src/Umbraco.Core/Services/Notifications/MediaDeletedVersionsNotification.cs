// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class MediaDeletedVersionsNotification : DeletedVersionsNotification<IMedia>
    {
        public MediaDeletedVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default) : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
        {
        }
    }
}
