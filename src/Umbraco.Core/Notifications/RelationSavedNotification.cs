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

    /// <summary>
    ///     Gets or sets a value indicating whether the relations were saved automatically
    ///     as a side-effect of content being saved (e.g. umbMedia, umbDocument, umbMember),
    ///     rather than explicitly via <see cref="IRelationService"/>.
    /// </summary>
    /// <remarks>
    ///     When <c>true</c>, the <see cref="IRelation.Id"/> of each saved entity may be <c>0</c>
    ///     because automatic relations are persisted via a bulk operation that does not return
    ///     database identities. <see cref="IRelation.ParentId"/>, <see cref="IRelation.ChildId"/>,
    ///     and <see cref="IRelation.RelationType"/> are always populated.
    /// </remarks>
    public bool IsAutomatic { get; set; }
}
