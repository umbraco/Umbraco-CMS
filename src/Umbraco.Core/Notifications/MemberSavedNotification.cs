// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class MemberSavedNotification : SavedNotification<IMember>
{
    public MemberSavedNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberSavedNotification(IEnumerable<IMember> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
