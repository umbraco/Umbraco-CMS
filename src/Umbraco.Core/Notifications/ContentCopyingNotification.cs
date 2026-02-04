// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before content is copied.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the copy operation.
///     The notification is published by the <see cref="Services.IContentService"/> after a copy object
///     has been created and its parentId updated and state set to unpublished, but before it is persisted.
/// </remarks>
public sealed class ContentCopyingNotification : CopyingNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentCopyingNotification"/> class.
    /// </summary>
    /// <param name="original">The original content item being copied.</param>
    /// <param name="copy">The copy of the content item.</param>
    /// <param name="parentId">The ID of the new parent.</param>
    /// <param name="parentKey">The key of the new parent.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentCopyingNotification(IContent original, IContent copy, int parentId, Guid? parentKey, EventMessages messages)
        : base(original, copy, parentId, parentKey, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentCopyingNotification"/> class.
    /// </summary>
    /// <param name="original">The original content item being copied.</param>
    /// <param name="copy">The copy of the content item.</param>
    /// <param name="parentId">The ID of the new parent.</param>
    /// <param name="messages">The event messages collection.</param>
    [Obsolete("Please use constructor that takes a parent key as well, scheduled for removal in v15")]
    public ContentCopyingNotification(IContent original, IContent copy, int parentId, EventMessages messages)
        : this(original, copy, parentId, null, messages)
    {
    }
}
