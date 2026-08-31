using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after one or more redirect URLs have been deleted.
/// </summary>
public class RedirectUrlDeletedNotification : DeletedNotification<IRedirectUrl>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlDeletedNotification" /> class with a single redirect URL.
    /// </summary>
    /// <param name="target">The redirect URL that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlDeletedNotification(IRedirectUrl target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlDeletedNotification" /> class with multiple redirect URLs.
    /// </summary>
    /// <param name="target">The redirect URLs that were deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlDeletedNotification(IEnumerable<IRedirectUrl> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
