// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after content has been copied.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentService"/> after the content object has been copied.
///     It is not cancelable since the copy operation has already completed.
/// </remarks>
public sealed class ContentCopiedNotification : CopiedNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentCopiedNotification"/> class.
    /// </summary>
    /// <param name="original">The original content item that was copied.</param>
    /// <param name="copy">The copy of the content item.</param>
    /// <param name="parentId">The ID of the new parent.</param>
    /// <param name="parentKey">The key of the new parent.</param>
    /// <param name="relateToOriginal">A value indicating whether the copy is related to the original.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentCopiedNotification(IContent original, IContent copy, int parentId, Guid? parentKey, bool relateToOriginal, EventMessages messages)
        : base(original, copy, parentId, parentKey, relateToOriginal, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentCopiedNotification"/> class.
    /// </summary>
    /// <param name="original">The original content item that was copied.</param>
    /// <param name="copy">The copy of the content item.</param>
    /// <param name="parentId">The ID of the new parent.</param>
    /// <param name="relateToOriginal">A value indicating whether the copy is related to the original.</param>
    /// <param name="messages">The event messages collection.</param>
    [Obsolete("Please use constructor that takes a parent key as well, scheduled for removal in v15")]
    public ContentCopiedNotification(IContent original, IContent copy, int parentId, bool relateToOriginal, EventMessages messages)
        : this(original, copy, parentId, null, relateToOriginal, messages)
    {
    }
}
