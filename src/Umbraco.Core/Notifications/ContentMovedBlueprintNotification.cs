// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is published after one or more content blueprints have been moved.
/// </summary>
public sealed class ContentMovedBlueprintNotification : MovedNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovedBlueprintNotification"/> class with a single move
    ///     operation.
    /// </summary>
    /// <param name="target">The move information for the blueprint that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovedBlueprintNotification(MoveEventInfo<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovedBlueprintNotification"/> class with multiple move
    ///     operations.
    /// </summary>
    /// <param name="target">The collection of move information for the blueprints that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovedBlueprintNotification(IEnumerable<MoveEventInfo<IContent>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
