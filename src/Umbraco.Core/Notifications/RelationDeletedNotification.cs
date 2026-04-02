// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a relation has been deleted.
/// </summary>
public class RelationDeletedNotification : DeletedNotification<IRelation>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationDeletedNotification"/> class
    ///     with a single relation.
    /// </summary>
    /// <param name="target">The relation that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public RelationDeletedNotification(IRelation target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationDeletedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IRelation"/> objects having been deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public RelationDeletedNotification(IEnumerable<IRelation> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
