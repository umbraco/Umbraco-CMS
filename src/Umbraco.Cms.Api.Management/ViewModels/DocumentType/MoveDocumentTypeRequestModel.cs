namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents the data required to move a document type to a new location within the system.
/// </summary>
public class MoveDocumentTypeRequestModel
{
    /// <summary>
    /// Gets or sets the target parent document type, referenced by ID, to which the document type will be moved.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
