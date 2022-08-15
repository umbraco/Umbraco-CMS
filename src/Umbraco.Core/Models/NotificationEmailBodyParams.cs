namespace Umbraco.Cms.Core.Models;

public class NotificationEmailBodyParams
{
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

    public string RecipientName { get; }

    public string Action { get; }

    public string ItemName { get; }

    public string ItemId { get; }

    public string ItemUrl { get; }

    /// <summary>
    ///     This will either be an HTML or text based summary depending on the email type being sent
    /// </summary>
    public string Summary { get; }

    public string EditedUser { get; }

    public string SiteUrl { get; }
}
