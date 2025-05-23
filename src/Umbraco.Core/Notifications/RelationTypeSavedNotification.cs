// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a relation type has been saved.
/// </summary>
public class RelationTypeSavedNotification : SavedNotification<IRelationType>
{
    public RelationTypeSavedNotification(IRelationType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationTypeSavedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IRelationType"/> objects having been saved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public RelationTypeSavedNotification(IEnumerable<IRelationType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
