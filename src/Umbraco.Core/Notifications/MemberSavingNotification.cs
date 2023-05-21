// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class MemberSavingNotification : SavingNotification<IMember>
{
    public MemberSavingNotification(IMember target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberSavingNotification(IEnumerable<IMember> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
