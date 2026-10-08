using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after a redirect URL has been saved.
/// </summary>
public class RedirectUrlSavedNotification : SavedNotification<IRedirectUrl>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlSavedNotification" /> class.
    /// </summary>
    /// <param name="target">The redirect URL that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlSavedNotification(IRedirectUrl target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlSavedNotification" /> class.
    /// </summary>
    /// <param name="target">The redirect URLs that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlSavedNotification(IEnumerable<IRedirectUrl> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
