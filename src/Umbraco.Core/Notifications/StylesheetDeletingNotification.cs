// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IFileService when the DeleteStylesheet method is called in the API.
/// </summary>
public class StylesheetDeletingNotification : DeletingNotification<IStylesheet>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetDeletingNotification"/> class
    ///     with a single stylesheet.
    /// </summary>
    /// <param name="target">The stylesheet being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public StylesheetDeletingNotification(IStylesheet target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetDeletingNotification"/> class
    ///     with multiple stylesheets.
    /// </summary>
    /// <param name="target">The stylesheets being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public StylesheetDeletingNotification(IEnumerable<IStylesheet> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
