// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class DeletedBlueprintNotification<T> : EnumerableObjectNotification<T>
    {
        public DeletedBlueprintNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public DeletedBlueprintNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> DeletedBlueprints => Target;
    }
}
