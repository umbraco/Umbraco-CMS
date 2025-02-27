// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the SavedBlueprint method is called in the API.
/// </summary>
public sealed class ContentSavedBlueprintNotification : ObjectNotification<IContent>
{
    public ContentSavedBlueprintNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Getting the saved blueprint <see cref="IContent"/> object.
    /// </summary>
    public IContent SavedBlueprint => Target;
}
