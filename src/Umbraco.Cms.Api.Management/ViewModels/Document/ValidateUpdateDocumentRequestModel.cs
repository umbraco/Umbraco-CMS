namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Model for validating update requests to a document.
/// </summary>
public class ValidateUpdateDocumentRequestModel : UpdateDocumentRequestModel
{
    /// <summary>
    /// Gets or sets the cultures to validate during the document update.
    /// </summary>
    public ISet<string>? Cultures { get; set; }
}
