namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents a response model for document notifications in the management API.
/// </summary>
public class DocumentNotificationResponseModel
{
    /// <summary>Gets or sets the identifier of the action.</summary>
    public required string ActionId { get; set; }

    /// <summary>
    /// Gets or sets the alias identifying the type of document notification.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>Indicates whether the user is subscribed to document notifications.</summary>
    public required bool Subscribed { get; set; }
}
