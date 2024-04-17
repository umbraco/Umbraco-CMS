// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the Copy method is called in the API.
/// The notification is published after a copy object has been created and had its parentId updated and its state has been set to unpublished.
/// </summary>
public sealed class ContentCopyingNotification : CopyingNotification<IContent>
{
    public ContentCopyingNotification(IContent original, IContent copy, int parentId, Guid? parentKey, EventMessages messages)
        : base(original, copy, parentId, parentKey, messages)
    {
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="ContentCopyingNotification"/>.
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
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    [Obsolete("Please use constructor that takes a parent key as well, scheduled for removal in v15")]
    public ContentCopyingNotification(IContent original, IContent copy, int parentId, EventMessages messages)
        : this(original, copy, parentId, null, messages)
    {
    }
}
