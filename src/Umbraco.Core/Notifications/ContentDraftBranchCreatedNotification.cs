// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after a named draft branch has been created.
/// </summary>
public class ContentDraftBranchCreatedNotification : CreatedNotification<IContentDraftBranch>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDraftBranchCreatedNotification"/> class.
    /// </summary>
    /// <param name="target">The branch that was created.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDraftBranchCreatedNotification(IContentDraftBranch target, EventMessages messages)
        : base(target, messages)
    {
    }
}
