// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the Copy method is called in the API.
/// The notification is published after the content object has been copied.
/// </summary>
public sealed class ContentCopiedNotification : CopiedNotification<IContent>
{
    public ContentCopiedNotification(IContent original, IContent copy, int parentId, Guid? parentKey, bool relateToOriginal, EventMessages messages)
        : base(original, copy, parentId, parentKey, relateToOriginal, messages)
    {
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="ContentCopiedNotification"/>.
    /// </summary>
    /// <param name="original">
    /// Gets the original <see cref="IContent"/> object.
    /// </param>
    /// <param name="copy">
    /// Gets the <see cref="IContent"/> object being copied.
    /// </param>
    /// <param name="parentId">
    /// Gets the ID of the parent of the <see cref="IContent"/> being copied.
    /// </param>
    /// <param name="relateToOriginal">
    /// Boolean indicating whether the copy was related to the orginal.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    [Obsolete("Please use constructor that takes a parent key as well, scheduled for removal in v15")]
    public ContentCopiedNotification(IContent original, IContent copy, int parentId, bool relateToOriginal, EventMessages messages)
        : this(original, copy, parentId, null, relateToOriginal, messages)
    {
    }
}
