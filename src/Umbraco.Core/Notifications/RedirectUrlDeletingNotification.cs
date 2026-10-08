using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before one or more redirect URLs are deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation
///     by setting <see cref="ICancelableNotification.Cancel" /> to <c>true</c>.
/// </remarks>
public class RedirectUrlDeletingNotification : DeletingNotification<IRedirectUrl>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlDeletingNotification" /> class with a single redirect URL.
    /// </summary>
    /// <param name="target">The redirect URL being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlDeletingNotification(IRedirectUrl target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlDeletingNotification" /> class with multiple redirect URLs.
    /// </summary>
    /// <param name="target">The redirect URLs being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlDeletingNotification(IEnumerable<IRedirectUrl> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
