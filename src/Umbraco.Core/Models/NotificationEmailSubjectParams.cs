namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the parameters used to construct a notification email subject line.
/// </summary>
public class NotificationEmailSubjectParams
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationEmailSubjectParams" /> class.
    /// </summary>
    /// <param name="siteUrl">The URL of the site.</param>
    /// <param name="action">The action that triggered the notification.</param>
    /// <param name="itemName">The name of the item involved in the action.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public NotificationEmailSubjectParams(string siteUrl, string? action, string? itemName)
    {
        SiteUrl = siteUrl ?? throw new ArgumentNullException(nameof(siteUrl));
        Action = action ?? throw new ArgumentNullException(nameof(action));
        ItemName = itemName ?? throw new ArgumentNullException(nameof(itemName));
    }

    /// <summary>
    ///     Gets the URL of the site.
    /// </summary>
    public string SiteUrl { get; }

    /// <summary>
    ///     Gets the action that triggered the notification.
    /// </summary>
    public string Action { get; }

    /// <summary>
    ///     Gets the name of the item involved in the action.
    /// </summary>
    public string ItemName { get; }
}
