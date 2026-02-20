// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a stylesheet has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IFileService"/> after the stylesheet has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public class StylesheetSavedNotification : SavedNotification<IStylesheet>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetSavedNotification"/> class with a single stylesheet.
    /// </summary>
    /// <param name="target">The stylesheet that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public StylesheetSavedNotification(IStylesheet target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetSavedNotification"/> class with multiple stylesheets.
    /// </summary>
    /// <param name="target">The collection of stylesheets that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public StylesheetSavedNotification(IEnumerable<IStylesheet> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
