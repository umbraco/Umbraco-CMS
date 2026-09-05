// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IElementService when the Rollback method is called in the API.
/// </summary>
public sealed class ElementRollingBackNotification : RollingBackNotification<IElement>
{
    public ElementRollingBackNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }
}
