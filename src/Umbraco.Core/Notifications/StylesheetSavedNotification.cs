// Copyright (c) Umbraco.
// See LICENSE for more details

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IFileService when the SaveStyleSheet method is called in the API, after the script has been saved.
/// </summary>
public class StylesheetSavedNotification : SavedNotification<IStylesheet>
{
    public StylesheetSavedNotification(IStylesheet target, EventMessages messages)
        : base(target, messages)
    {
    }

    public StylesheetSavedNotification(IEnumerable<IStylesheet> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
