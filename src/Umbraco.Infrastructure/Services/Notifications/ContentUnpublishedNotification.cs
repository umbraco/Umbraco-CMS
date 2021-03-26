// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class ContentUnpublishedNotification : EnumerableObjectNotification<IContent>
    {
        public ContentUnpublishedNotification(IContent target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentUnpublishedNotification(IEnumerable<IContent> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<IContent> UnpublishedEntities => Target;
    }
}
