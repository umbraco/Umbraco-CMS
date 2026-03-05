// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a relation type is being deleted.
/// </summary>
public class RelationTypeDeletingNotification : DeletingNotification<IRelationType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationTypeDeletingNotification"/> class
    ///     with a single relation type.
    /// </summary>
    /// <param name="target">The relation type being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public RelationTypeDeletingNotification(IRelationType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationTypeDeletingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IRelationType"/> objects being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public RelationTypeDeletingNotification(IEnumerable<IRelationType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
