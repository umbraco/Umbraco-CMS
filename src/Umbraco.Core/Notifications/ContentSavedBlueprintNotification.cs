// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the IContentService when the SavedBlueprint method is called in the API.
/// </summary>
public sealed class ContentSavedBlueprintNotification : ObjectNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSavedBlueprintNotification"/> class.
    /// </summary>
    /// <param name="target">The content blueprint that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentSavedBlueprintNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSavedBlueprintNotification"/> class
    ///     with information about the original content.
    /// </summary>
    /// <param name="target">The content blueprint that was saved.</param>
    /// <param name="createdFromContent">The original content from which the blueprint was created, if any.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentSavedBlueprintNotification(IContent target, IContent? createdFromContent, EventMessages messages)
        : base(target, messages)
    {
        CreatedFromContent = createdFromContent;
    }

    /// <summary>
    ///     Getting the saved blueprint <see cref="IContent"/> object.
    /// </summary>
    public IContent SavedBlueprint => Target;

    /// <summary>
    ///     Gets the original content from which the blueprint was created, if any.
    /// </summary>
    public IContent? CreatedFromContent { get; }
}
