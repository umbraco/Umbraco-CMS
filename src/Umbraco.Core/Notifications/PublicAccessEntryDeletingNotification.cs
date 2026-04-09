// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before public access entries are deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public sealed class PublicAccessEntryDeletingNotification : DeletingNotification<PublicAccessEntry>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessEntryDeletingNotification"/> class
    ///     with a single entry.
    /// </summary>
    /// <param name="target">The public access entry being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public PublicAccessEntryDeletingNotification(PublicAccessEntry target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessEntryDeletingNotification"/> class
    ///     with multiple entries.
    /// </summary>
    /// <param name="target">The public access entries being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public PublicAccessEntryDeletingNotification(IEnumerable<PublicAccessEntry> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
