// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when changes occur in the media tree structure.
/// </summary>
/// <remarks>
///     This notification is published when media items are added, removed, moved, or have their
///     structure modified in the media tree. It is used for cache invalidation and tree synchronization.
/// </remarks>
public class MediaTreeChangeNotification : TreeChangeNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTreeChangeNotification"/> class
    ///     with a single tree change.
    /// </summary>
    /// <param name="target">The tree change that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTreeChangeNotification(TreeChange<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTreeChangeNotification"/> class
    ///     with multiple tree changes.
    /// </summary>
    /// <param name="target">The collection of tree changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTreeChangeNotification(IEnumerable<TreeChange<IMedia>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTreeChangeNotification"/> class
    ///     for multiple media items with the same change type.
    /// </summary>
    /// <param name="target">The media items that changed.</param>
    /// <param name="changeTypes">The type of changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTreeChangeNotification(
        IEnumerable<IMedia> target,
        TreeChangeTypes changeTypes,
        EventMessages messages)
        : base(target.Select(x => new TreeChange<IMedia>(x, changeTypes)), messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTreeChangeNotification"/> class
    ///     for a single media item.
    /// </summary>
    /// <param name="target">The media item that changed.</param>
    /// <param name="changeTypes">The type of changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTreeChangeNotification(IMedia target, TreeChangeTypes changeTypes, EventMessages messages)
        : base(new TreeChange<IMedia>(target, changeTypes), messages)
    {
    }
}
