// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class SavedBlueprintNotification<T> : ObjectNotification<T> where T : class
    {
        public SavedBlueprintNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T SavedBlueprint => Target;
    }
}
