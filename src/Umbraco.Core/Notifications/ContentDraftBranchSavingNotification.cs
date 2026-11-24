// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before edits to a named draft branch's content are saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>. The underlying content
///     save (and its own <c>ContentSavingNotification</c>/<c>ElementSavingNotification</c>) still fires
///     separately - this notification is specifically about the branch being edited.
/// </remarks>
public class ContentDraftBranchSavingNotification : SavingNotification<IContentDraftBranch>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDraftBranchSavingNotification"/> class.
    /// </summary>
    /// <param name="target">The branch being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDraftBranchSavingNotification(IContentDraftBranch target, EventMessages messages)
        : base(target, messages)
    {
    }
}
