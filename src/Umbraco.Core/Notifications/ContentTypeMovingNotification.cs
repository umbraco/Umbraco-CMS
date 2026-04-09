// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the ContentTypeService when the Move method is called in the API.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the move operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class ContentTypeMovingNotification : MovingNotification<IContentType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeMovingNotification"/> class
    ///     with a single move operation.
    /// </summary>
    /// <param name="target">The move event information for the content type being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeMovingNotification(MoveEventInfo<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeMovingNotification"/> class
    ///     with multiple move operations.
    /// </summary>
    /// <param name="target">The move event information for the content types being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeMovingNotification(IEnumerable<MoveEventInfo<IContentType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
