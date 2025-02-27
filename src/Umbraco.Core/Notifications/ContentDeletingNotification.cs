// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the DeleteContentOfType, Delete and EmptyRecycleBin methods are called in the API.
/// </summary>
public sealed class ContentDeletingNotification : DeletingNotification<IContent>
{
    public ContentDeletingNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentDeletingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
