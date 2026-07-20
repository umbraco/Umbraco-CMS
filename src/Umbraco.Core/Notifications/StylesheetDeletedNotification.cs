// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IFileService when the DeleteStylesheet method is called in the API, after the stylesheet has been deleted.
/// </summary>
public class StylesheetDeletedNotification : DeletedNotification<IStylesheet>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetDeletedNotification"/> class
    ///     with a single stylesheet.
    /// </summary>
    /// <param name="target">The stylesheet that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public StylesheetDeletedNotification(IStylesheet target, EventMessages messages)
        : base(target, messages)
    {
    }
}
