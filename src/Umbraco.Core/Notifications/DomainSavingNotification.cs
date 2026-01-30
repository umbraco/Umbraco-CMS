// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before domains are saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class DomainSavingNotification : SavingNotification<IDomain>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainSavingNotification"/> class
    ///     with a single domain.
    /// </summary>
    /// <param name="target">The domain being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DomainSavingNotification(IDomain target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainSavingNotification"/> class
    ///     with multiple domains.
    /// </summary>
    /// <param name="target">The domains being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DomainSavingNotification(IEnumerable<IDomain> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
