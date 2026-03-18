namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the data required to update notifications for a document.
/// </summary>
public class UpdateDocumentNotificationsRequestModel
{
    /// <summary>
    /// Gets or sets the IDs of the subscribed actions for document notifications.
    /// </summary>
    public required string[] SubscribedActionIds { get; set; }
}
