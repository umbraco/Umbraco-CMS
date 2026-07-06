using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before a redirect URL is saved.
/// </summary>
public class RedirectUrlSavingNotification : SavingNotification<IRedirectUrl>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlSavingNotification" /> class.
    /// </summary>
    /// <param name="target">The redirect URL being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlSavingNotification(IRedirectUrl target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlSavingNotification" /> class.
    /// </summary>
    /// <param name="target">The redirect URLs being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlSavingNotification(IEnumerable<IRedirectUrl> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
