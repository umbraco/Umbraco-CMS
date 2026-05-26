using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before a redirect URL is created.
/// </summary>
public class RedirectUrlCreatingNotification : CreatingNotification<IRedirectUrl>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrlCreatingNotification" /> class.
    /// </summary>
    /// <param name="target">The redirect URL being created.</param>
    /// <param name="newUrl">The current (new) URL of the content item the redirect is for.</param>
    /// <param name="messages">The event messages collection.</param>
    public RedirectUrlCreatingNotification(IRedirectUrl target, string? newUrl, EventMessages messages)
        : base(target, messages) =>
        NewUrl = newUrl;

    /// <summary>
    ///     Gets the key of the content item the redirect is being created for.
    /// </summary>
    public Guid ContentKey => CreatedEntity.ContentKey;

    /// <summary>
    ///     Gets the URL the redirect is being created from.
    /// </summary>
    public string OldUrl => CreatedEntity.Url;

    /// <summary>
    ///     Gets the URL the redirect is being created to.
    /// </summary>
    public string? NewUrl { get; }
}
