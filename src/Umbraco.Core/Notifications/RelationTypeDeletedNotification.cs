// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is published when a relation type has been deleted.
/// </summary>
public class RelationTypeDeletedNotification : DeletedNotification<IRelationType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationTypeDeletedNotification"/> class
    ///     with a single relation type.
    /// </summary>
    /// <param name="target">The relation type that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public RelationTypeDeletedNotification(IRelationType target, EventMessages messages)
        : base(target, messages)
    {
    }
}
