// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before a named draft branch is promoted (its content copied forward onto the
///     node's default draft via a plain save - no publish happens).
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the promotion by setting
///     <see cref="ICancelableNotification.Cancel"/> to <c>true</c>. Promoting a branch never publishes
///     anything, so there is no corresponding <c>ContentPublishingNotification</c> firing alongside this one -
///     if the default draft is later actually published (via an editorial action or a scheduled publish),
///     that is an entirely separate, ordinary content operation with no branch awareness at all.
/// </remarks>
public class ContentDraftBranchPromotingNotification : CancelableObjectNotification<IContentDraftBranch>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDraftBranchPromotingNotification"/> class.
    /// </summary>
    /// <param name="target">The branch being promoted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDraftBranchPromotingNotification(IContentDraftBranch target, EventMessages messages)
        : base(target, messages)
    {
    }
}
