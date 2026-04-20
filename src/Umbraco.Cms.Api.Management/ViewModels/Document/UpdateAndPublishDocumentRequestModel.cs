namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents a request model used for updating and publishing a document via the API.
/// </summary>
public class UpdateAndPublishDocumentRequestModel : UpdateDocumentRequestModel
{
    /// <summary>
    /// The cultures to publish after updating the document.
    /// </summary>
    public string[] CulturesToPublish { get; set; } = [];
}
