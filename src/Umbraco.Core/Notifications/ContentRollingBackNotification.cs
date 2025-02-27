// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the Rollback method is called in the API.
/// </summary>
public sealed class ContentRollingBackNotification : RollingBackNotification<IContent>
{
    public ContentRollingBackNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
}
