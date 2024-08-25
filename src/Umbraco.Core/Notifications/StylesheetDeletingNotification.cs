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
    public StylesheetDeletingNotification(IStylesheet target, EventMessages messages)
        : base(target, messages)
    {
    }

    public StylesheetDeletingNotification(IEnumerable<IStylesheet> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
