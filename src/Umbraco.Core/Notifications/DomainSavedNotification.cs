// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after domains have been saved.
/// </summary>
/// <remarks>
///     This notification is published after domains have been successfully saved,
///     allowing handlers to react for auditing or cache invalidation purposes.
/// </remarks>
public class DomainSavedNotification : SavedNotification<IDomain>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainSavedNotification"/> class
    ///     with a single domain.
    /// </summary>
    /// <param name="target">The domain that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DomainSavedNotification(IDomain target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainSavedNotification"/> class
    ///     with multiple domains.
    /// </summary>
    /// <param name="target">The domains that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DomainSavedNotification(IEnumerable<IDomain> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
