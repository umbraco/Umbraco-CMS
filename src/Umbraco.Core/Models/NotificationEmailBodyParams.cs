namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the parameters used for generating a notification email body.
/// </summary>
public class NotificationEmailBodyParams
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationEmailBodyParams" /> class.
    /// </summary>
    /// <param name="recipientName">The name of the email recipient.</param>
    /// <param name="action">The action that triggered the notification (e.g., "published", "updated").</param>
    /// <param name="itemName">The name of the content item affected.</param>
    /// <param name="itemId">The unique identifier of the content item.</param>
    /// <param name="itemUrl">The URL to the content item in the backoffice.</param>
    /// <param name="editedUser">The name of the user who performed the action.</param>
    /// <param name="siteUrl">The base URL of the site.</param>
    /// <param name="summary">The summary of changes, either HTML or text based depending on email type.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public NotificationEmailBodyParams(string? recipientName, string? action, string? itemName, string itemId, string itemUrl, string? editedUser, string siteUrl, string summary)
    {
        RecipientName = recipientName ?? throw new ArgumentNullException(nameof(recipientName));
        Action = action ?? throw new ArgumentNullException(nameof(action));
        ItemName = itemName ?? throw new ArgumentNullException(nameof(itemName));
        ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
        ItemUrl = itemUrl ?? throw new ArgumentNullException(nameof(itemUrl));
        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        EditedUser = editedUser ?? throw new ArgumentNullException(nameof(editedUser));
        SiteUrl = siteUrl ?? throw new ArgumentNullException(nameof(siteUrl));
    }

    /// <summary>
    ///     Gets the name of the email recipient.
    /// </summary>
    public string RecipientName { get; }

    /// <summary>
    ///     Gets the action that triggered the notification (e.g., "published", "updated").
    /// </summary>
    public string Action { get; }

    /// <summary>
    ///     Gets the name of the content item affected.
    /// </summary>
    public string ItemName { get; }

    /// <summary>
    ///     Gets the unique identifier of the content item.
    /// </summary>
    public string ItemId { get; }

    /// <summary>
    ///     Gets the URL to the content item in the backoffice.
    /// </summary>
    public string ItemUrl { get; }

    /// <summary>
    ///     This will either be an HTML or text based summary depending on the email type being sent
    /// </summary>
    public string Summary { get; }

    /// <summary>
    ///     Gets the name of the user who performed the action.
    /// </summary>
    public string EditedUser { get; }

    /// <summary>
    ///     Gets the base URL of the site.
    /// </summary>
    public string SiteUrl { get; }
}
