// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the DeletedBlueprint method is called in the API.
/// </summary>
public sealed class ContentDeletedBlueprintNotification : EnumerableObjectNotification<IContent>
{
    public ContentDeletedBlueprintNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentDeletedBlueprintNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
    /// <summary>
    /// The collection of deleted blueprint IContent.
    /// </summary>
    public IEnumerable<IContent> DeletedBlueprints => Target;
}
