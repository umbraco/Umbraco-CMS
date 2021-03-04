// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class SavingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public SavingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public SavingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SavedEntities => Target;
    }
}
