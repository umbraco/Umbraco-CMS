// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after domains have been deleted.
/// </summary>
/// <remarks>
///     This notification is published after domains have been successfully deleted,
///     allowing handlers to react for auditing or cache invalidation purposes.
/// </remarks>
public class DomainDeletedNotification : DeletedNotification<IDomain>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainDeletedNotification"/> class
    ///     with a single domain.
    /// </summary>
    /// <param name="target">The domain that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public DomainDeletedNotification(IDomain target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainDeletedNotification"/> class
    ///     with multiple domains.
    /// </summary>
    /// <param name="target">The domains that were deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public DomainDeletedNotification(IEnumerable<IDomain> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
