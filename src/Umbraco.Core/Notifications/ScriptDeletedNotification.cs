// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IFileService when the DeleteScript method is called in the API, after the script has been deleted.
/// </summary>
public class ScriptDeletedNotification : DeletedNotification<IScript>
{
    public ScriptDeletedNotification(IScript target, EventMessages messages)
        : base(target, messages)
    {
    }
}
