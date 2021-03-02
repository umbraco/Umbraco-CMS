// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class DeletedNotification<T> : EnumerableObjectNotification<T>
    {
        public DeletedNotification(T target, EventMessages messages) : base(target, messages) => MediaFilesToDelete = new List<string>();

        public IEnumerable<T> DeletedEntities => Target;

        public List<string> MediaFilesToDelete { get; }
    }
}
