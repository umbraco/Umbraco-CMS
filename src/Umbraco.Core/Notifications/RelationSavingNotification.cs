// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a relation is being saved.
/// </summary>
public class RelationSavingNotification : SavingNotification<IRelation>
{
    public RelationSavingNotification(IRelation target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationSavingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IRelation"/> objects being saved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public RelationSavingNotification(IEnumerable<IRelation> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
