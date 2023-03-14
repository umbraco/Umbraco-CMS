// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentCopyingNotification : CopyingNotification<IContent>
{
    public ContentCopyingNotification(IContent original, IContent copy, int parentId, EventMessages messages)
        : base(original, copy, parentId, messages)
    {
    }
}
