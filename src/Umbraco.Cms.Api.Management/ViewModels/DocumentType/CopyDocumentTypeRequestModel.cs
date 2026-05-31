namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents the data required to copy an existing document type in the system.
/// </summary>
public class CopyDocumentTypeRequestModel
{
    /// <summary>
    /// Gets or sets the target location (by ID) where the document type will be copied.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
