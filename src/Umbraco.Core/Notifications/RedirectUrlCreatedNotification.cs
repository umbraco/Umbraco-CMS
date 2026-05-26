using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after a redirect URL has been created.
/// </summary>
public class RedirectUrlCreatedNotification : CreatedNotification<IRedirectUrl>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlCreatedNotification" /> class.
    /// </summary>
    /// <param name="target">The redirect URL that was created.</param>
    /// <param name="newUrl">The new URL of the content item the redirect is for.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlCreatedNotification(IRedirectUrl target, string? newUrl, EventMessages messages)
        : base(target, messages) =>
        NewUrl = newUrl;

    /// <summary>
    ///     Gets the key of the content item the redirect was created for.
    /// </summary>
    public Guid ContentKey => CreatedEntity.ContentKey;

    /// <summary>
    ///     Gets the URL the redirect was created from.
    /// </summary>
    public string OldUrl => CreatedEntity.Url;

    /// <summary>
    ///     Gets the URL the redirect was created to.
    /// </summary>
    public string? NewUrl { get; }
}
