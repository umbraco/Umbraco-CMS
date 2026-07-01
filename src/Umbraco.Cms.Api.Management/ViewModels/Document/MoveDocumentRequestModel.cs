namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the request model used when moving a document to a new location via the management API.
/// </summary>
public class MoveDocumentRequestModel
{
    /// <summary>
    /// Gets or sets the target location, specified by ID, to which the document should be moved.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
