// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentCopyingNotification : CopyingNotification<IContent>
{
    public ContentCopyingNotification(IContent original, IContent copy, int parentId, Guid? parentKey, EventMessages messages)
        : base(original, copy, parentId, parentKey, messages)
    {
    }

    [Obsolete("Please use constructor that takes a parent key as well, scheduled for removal in v15")]
    public ContentCopyingNotification(IContent original, IContent copy, int parentId, EventMessages messages)
        : this(original, copy, parentId, null, messages)
    {
    }
}
