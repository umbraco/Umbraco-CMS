// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class DeletingVersionsNotification<T> : DeletedVersionsNotificationBase<T>, ICancelableNotification
    where T : class
{
    protected DeletingVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
        : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
    {
    }

    public bool Cancel { get; set; }
}
