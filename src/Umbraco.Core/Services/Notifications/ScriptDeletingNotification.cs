// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class ScriptDeletingNotification : DeletingNotification<IScript>
    {
        public ScriptDeletingNotification(IScript target, EventMessages messages) : base(target, messages)
        {
        }

        public ScriptDeletingNotification(IEnumerable<IScript> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
