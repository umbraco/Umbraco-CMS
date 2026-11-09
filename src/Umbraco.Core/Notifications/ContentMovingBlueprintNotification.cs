// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A cancelable notification that is published before one or more content blueprints are moved.
/// </summary>
public sealed class ContentMovingBlueprintNotification : MovingNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovingBlueprintNotification"/> class with a single move
    ///     operation.
    /// </summary>
    /// <param name="target">The move information for the blueprint being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovingBlueprintNotification(MoveEventInfo<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovingBlueprintNotification"/> class with multiple move
    ///     operations.
    /// </summary>
    /// <param name="target">The collection of move information for the blueprints being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovingBlueprintNotification(IEnumerable<MoveEventInfo<IContent>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
