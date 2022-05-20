// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentCopiedNotification : CopiedNotification<IContent>
{
    public ContentCopiedNotification(IContent original, IContent copy, int parentId, bool relateToOriginal, EventMessages messages)
        : base(original, copy, parentId, relateToOriginal, messages)
    {
    }
}
