// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a relation type is being saved.
/// </summary>
public class RelationTypeSavingNotification : SavingNotification<IRelationType>
{
    public RelationTypeSavingNotification(IRelationType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationTypeSavingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IRelationType"/> objects being saved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public RelationTypeSavingNotification(IEnumerable<IRelationType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
