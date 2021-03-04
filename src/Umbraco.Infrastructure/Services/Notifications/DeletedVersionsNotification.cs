// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class DeletedVersionsNotification<T> : DeletedVersionsNotificationBase<T> where T : class
    {
        public DeletedVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
            : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
        {
        }
    }
}
