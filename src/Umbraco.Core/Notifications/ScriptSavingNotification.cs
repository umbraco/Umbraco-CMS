// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class ScriptSavingNotification : SavingNotification<IScript>
{
    public ScriptSavingNotification(IScript target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ScriptSavingNotification(IEnumerable<IScript> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
