namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the API request model used for creating and publishing a new content document in Umbraco.
/// </summary>
public class CreateAndPublishDocumentRequestModel : CreateDocumentRequestModel
{
    /// <summary>
    /// The cultures to publish after creating the document.
    /// </summary>
    public string[] CulturesToPublish { get; set; } = [];
}
