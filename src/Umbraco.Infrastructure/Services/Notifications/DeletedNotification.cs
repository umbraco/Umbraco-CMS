// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public abstract class DeletedNotification<T> : EnumerableObjectNotification<T>
    {
        protected DeletedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> DeletedEntities => Target;
    }
}
