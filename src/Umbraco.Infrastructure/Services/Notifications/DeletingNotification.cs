// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class DeletingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public DeletingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public DeletingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> DeletedEntities => Target;
    }
}
