// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class SavedNotification<T> : EnumerableObjectNotification<T>
    {
        public SavedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public SavedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SavedEntities => Target;
    }
}
