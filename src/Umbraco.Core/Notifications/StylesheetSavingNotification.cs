// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a stylesheet is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IFileService"/> before the stylesheet is persisted.
/// </remarks>
public class StylesheetSavingNotification : SavingNotification<IStylesheet>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetSavingNotification"/> class with a single stylesheet.
    /// </summary>
    /// <param name="target">The stylesheet being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public StylesheetSavingNotification(IStylesheet target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetSavingNotification"/> class with multiple stylesheets.
    /// </summary>
    /// <param name="target">The collection of stylesheets being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public StylesheetSavingNotification(IEnumerable<IStylesheet> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
