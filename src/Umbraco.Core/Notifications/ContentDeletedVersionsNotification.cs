// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentDeletedVersionsNotification : DeletedVersionsNotification<IContent>
{
    public ContentDeletedVersionsNotification(
        int id,
        EventMessages messages,
        int specificVersion = default,
        bool deletePriorVersions = false,
        DateTime dateToRetain = default)
        : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
    {
    }
}
