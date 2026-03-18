// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before domains are deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class DomainDeletingNotification : DeletingNotification<IDomain>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainDeletingNotification"/> class
    ///     with a single domain.
    /// </summary>
    /// <param name="target">The domain being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public DomainDeletingNotification(IDomain target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainDeletingNotification"/> class
    ///     with multiple domains.
    /// </summary>
    /// <param name="target">The domains being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public DomainDeletingNotification(IEnumerable<IDomain> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
