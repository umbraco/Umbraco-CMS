namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the API request model for creating a new named draft branch of a document.
/// </summary>
public class CreateDocumentDraftBranchRequestModel
{
    /// <summary>
    /// Gets or sets the editor-facing name of the new draft branch (e.g. "Black Friday").
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
