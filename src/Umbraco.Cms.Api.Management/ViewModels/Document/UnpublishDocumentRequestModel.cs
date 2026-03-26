namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// A request model used to unpublish a document.
/// </summary>
public class UnpublishDocumentRequestModel
{
    /// <summary>
    /// Gets or sets the set of cultures for which the document should be unpublished.
    /// </summary>
    public ISet<string>? Cultures { get; set; }
}
