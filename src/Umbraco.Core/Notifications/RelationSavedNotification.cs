// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a relation has been saved.
/// </summary>
public class RelationSavedNotification : SavedNotification<IRelation>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationSavedNotification"/> class
    ///     with a single relation.
    /// </summary>
    /// <param name="target">The relation that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public RelationSavedNotification(IRelation target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationSavedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IRelation"/> objects having been saved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public RelationSavedNotification(IEnumerable<IRelation> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
