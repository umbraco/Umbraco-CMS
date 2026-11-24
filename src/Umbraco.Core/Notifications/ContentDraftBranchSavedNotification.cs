// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after edits to a named draft branch's content have been saved.
/// </summary>
public class ContentDraftBranchSavedNotification : SavedNotification<IContentDraftBranch>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDraftBranchSavedNotification"/> class.
    /// </summary>
    /// <param name="target">The branch that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDraftBranchSavedNotification(IContentDraftBranch target, EventMessages messages)
        : base(target, messages)
    {
    }
}
