// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class DeletedVersionsNotificationBase<T> : StatefulNotification
    where T : class
{
    protected DeletedVersionsNotificationBase(
        int id,
        EventMessages messages,
        int specificVersion = default,
        bool deletePriorVersions = false,
        DateTime dateToRetain = default)
    {
        Id = id;
        Messages = messages;
        SpecificVersion = specificVersion;
        DeletePriorVersions = deletePriorVersions;
        DateToRetain = dateToRetain;
    }

    public int Id { get; }

    public EventMessages Messages { get; }

    public int SpecificVersion { get; }

    public bool DeletePriorVersions { get; }

    public DateTime DateToRetain { get; }
}
