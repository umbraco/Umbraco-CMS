// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after public access entries have been deleted.
/// </summary>
/// <remarks>
///     Public access entries control which content nodes are restricted and require
///     authentication. This notification is published after entries have been deleted.
/// </remarks>
public sealed class PublicAccessEntryDeletedNotification : DeletedNotification<PublicAccessEntry>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessEntryDeletedNotification"/> class.
    /// </summary>
    /// <param name="target">The public access entry that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public PublicAccessEntryDeletedNotification(PublicAccessEntry target, EventMessages messages)
        : base(target, messages)
    {
    }
}
