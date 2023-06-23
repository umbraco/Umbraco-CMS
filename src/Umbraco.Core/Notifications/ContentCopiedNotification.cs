// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentCopiedNotification : CopiedNotification<IContent>
{
    public ContentCopiedNotification(IContent original, IContent copy, int parentId, Guid? parentKey, bool relateToOriginal, EventMessages messages)
        : base(original, copy, parentId, parentKey, relateToOriginal, messages)
    {
    }

    [Obsolete("Please use constructor that takes a parent key as well, scheduled for removal in v15")]
    public ContentCopiedNotification(IContent original, IContent copy, int parentId, bool relateToOriginal, EventMessages messages)
        : this(original, copy, parentId, null, relateToOriginal, messages)
    {
    }
}
