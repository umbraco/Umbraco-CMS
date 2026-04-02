// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when changes occur in the content tree structure.
/// </summary>
/// <remarks>
///     This notification is published when content items are added, removed, moved, or have their
///     structure modified in the content tree. It is used for cache invalidation and tree synchronization.
/// </remarks>
public class ContentTreeChangeNotification : TreeChangeNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTreeChangeNotification"/> class
    ///     with a single tree change.
    /// </summary>
    /// <param name="target">The tree change that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTreeChangeNotification(TreeChange<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTreeChangeNotification"/> class
    ///     with multiple tree changes.
    /// </summary>
    /// <param name="target">The collection of tree changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTreeChangeNotification(IEnumerable<TreeChange<IContent>> target, EventMessages messages)
        : base(
        target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTreeChangeNotification"/> class
    ///     for multiple content items with the same change type.
    /// </summary>
    /// <param name="target">The content items that changed.</param>
    /// <param name="changeTypes">The type of changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTreeChangeNotification(
        IEnumerable<IContent> target,
        TreeChangeTypes changeTypes,
        EventMessages messages)
        : base(target.Select(x => new TreeChange<IContent>(x, changeTypes)), messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTreeChangeNotification"/> class
    ///     for a single content item.
    /// </summary>
    /// <param name="target">The content item that changed.</param>
    /// <param name="changeTypes">The type of changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTreeChangeNotification(
        IContent target,
        TreeChangeTypes changeTypes,
        EventMessages messages)
        : base(new TreeChange<IContent>(target, changeTypes), messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTreeChangeNotification"/> class
    ///     for a single content item with culture-specific publishing information.
    /// </summary>
    /// <param name="target">The content item that changed.</param>
    /// <param name="changeTypes">The type of changes that occurred.</param>
    /// <param name="publishedCultures">The cultures that were published, if any.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished, if any.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTreeChangeNotification(
        IContent target,
        TreeChangeTypes changeTypes,
        IEnumerable<string>? publishedCultures,
        IEnumerable<string>? unpublishedCultures,
        EventMessages messages)
        : base(new TreeChange<IContent>(target, changeTypes, publishedCultures, unpublishedCultures), messages)
    {
    }
}
