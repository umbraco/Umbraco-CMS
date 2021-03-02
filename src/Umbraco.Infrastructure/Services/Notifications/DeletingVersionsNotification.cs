// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class DeletingVersionsNotification<T> : DeletedVersionsNotification<T>, ICancelableNotification where T : class
    {
        public DeletingVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
            : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
        {
        }

        public bool Cancel { get; set; }
    }
}
