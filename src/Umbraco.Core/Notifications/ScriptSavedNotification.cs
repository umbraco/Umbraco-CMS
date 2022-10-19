// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class ScriptSavedNotification : SavedNotification<IScript>
{
    public ScriptSavedNotification(IScript target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ScriptSavedNotification(IEnumerable<IScript> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
