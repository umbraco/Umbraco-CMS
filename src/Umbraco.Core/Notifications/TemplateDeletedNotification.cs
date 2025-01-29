// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IFileService when the DeleteTemplate method is called in the API, after the template has been deleted.
/// </summary>
public class TemplateDeletedNotification : DeletedNotification<ITemplate>
{
    public TemplateDeletedNotification(ITemplate target, EventMessages messages)
        : base(target, messages)
    {
    }
}
