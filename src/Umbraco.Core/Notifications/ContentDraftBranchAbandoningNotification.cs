// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before a named draft branch is abandoned.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the branch from being abandoned
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>. Abandoning is now a hard,
///     immediate row deletion - not a status change. This remains a dedicated notification (rather than the
///     generic <c>DeletingNotification{T}</c>) because the branch entity doesn't otherwise participate in
///     that generic delete pipeline.
/// </remarks>
public class ContentDraftBranchAbandoningNotification : CancelableObjectNotification<IContentDraftBranch>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDraftBranchAbandoningNotification"/> class.
    /// </summary>
    /// <param name="target">The branch being abandoned.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDraftBranchAbandoningNotification(IContentDraftBranch target, EventMessages messages)
        : base(target, messages)
    {
    }
}
