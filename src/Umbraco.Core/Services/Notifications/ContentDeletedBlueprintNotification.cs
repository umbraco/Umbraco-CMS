// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class ContentDeletedBlueprintNotification : EnumerableObjectNotification<IContent>
    {
        public ContentDeletedBlueprintNotification(IContent target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentDeletedBlueprintNotification(IEnumerable<IContent> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<IContent> DeletedBlueprints => Target;
    }
}
