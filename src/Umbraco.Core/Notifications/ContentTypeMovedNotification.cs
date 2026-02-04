// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the ContentTypeService when the Move method is called in the API, after the entities have been moved.
/// </summary>
public class ContentTypeMovedNotification : MovedNotification<IContentType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeMovedNotification"/> class
    ///     with a single move operation.
    /// </summary>
    /// <param name="target">The move event information for the content type that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeMovedNotification(MoveEventInfo<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeMovedNotification"/> class
    ///     with multiple move operations.
    /// </summary>
    /// <param name="target">The move event information for the content types that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeMovedNotification(IEnumerable<MoveEventInfo<IContentType>> target, EventMessages messages)
        : base(
        target, messages)
    {
    }
}
