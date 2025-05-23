// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a relation is being deleted.
/// </summary>
public class RelationDeletingNotification : DeletingNotification<IRelation>
{
    public RelationDeletingNotification(IRelation target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationDeletingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IRelation"/> objects being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public RelationDeletingNotification(IEnumerable<IRelation> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
