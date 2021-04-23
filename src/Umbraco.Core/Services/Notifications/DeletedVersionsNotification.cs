// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public abstract class DeletedVersionsNotification<T> : DeletedVersionsNotificationBase<T> where T : class
    {
        protected DeletedVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
            : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
        {
        }
    }
}
