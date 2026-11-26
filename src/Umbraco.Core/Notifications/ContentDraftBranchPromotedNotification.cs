// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after a named draft branch has been promoted. Promoting is an immediate,
///     permanent deletion of the branch row (mirroring <see cref="ContentDraftBranchAbandonedNotification"/>'s
///     own semantics) - the branch carried by this notification no longer exists in the repository by the time
///     handlers observe it.
/// </summary>
public class ContentDraftBranchPromotedNotification : ObjectNotification<IContentDraftBranch>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDraftBranchPromotedNotification"/> class.
    /// </summary>
    /// <param name="target">The branch that was promoted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDraftBranchPromotedNotification(IContentDraftBranch target, EventMessages messages)
        : base(target, messages)
    {
    }
}
