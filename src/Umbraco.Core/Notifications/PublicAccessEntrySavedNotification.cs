// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after public access entries have been saved.
/// </summary>
/// <remarks>
///     Public access entries control which content nodes are restricted and require
///     authentication. This notification is published after entries have been saved.
/// </remarks>
public sealed class PublicAccessEntrySavedNotification : SavedNotification<PublicAccessEntry>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessEntrySavedNotification"/> class
    ///     with a single entry.
    /// </summary>
    /// <param name="target">The public access entry that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public PublicAccessEntrySavedNotification(PublicAccessEntry target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessEntrySavedNotification"/> class
    ///     with multiple entries.
    /// </summary>
    /// <param name="target">The public access entries that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public PublicAccessEntrySavedNotification(IEnumerable<PublicAccessEntry> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
