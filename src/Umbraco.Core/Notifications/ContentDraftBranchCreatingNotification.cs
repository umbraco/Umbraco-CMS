// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before a named draft branch is created.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the branch from being created
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class ContentDraftBranchCreatingNotification : CreatingNotification<IContentDraftBranch>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDraftBranchCreatingNotification"/> class.
    /// </summary>
    /// <param name="target">The branch being created.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDraftBranchCreatingNotification(IContentDraftBranch target, EventMessages messages)
        : base(target, messages)
    {
    }
}
