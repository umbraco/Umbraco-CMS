namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class UpdateAndPublishDocumentRequestModel : UpdateDocumentRequestModel
{
    /// <summary>
    /// Gets or sets the cultures to publish immediately. Use null for invariant content.
    /// </summary>
    public required IEnumerable<string?> Cultures { get; set; }
}
