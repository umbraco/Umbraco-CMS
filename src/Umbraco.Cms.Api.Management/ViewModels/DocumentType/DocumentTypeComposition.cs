using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents a document type composition in the Umbraco CMS Management API.
/// </summary>
public class DocumentTypeComposition
{
    /// <summary>
    /// Gets or sets a reference to the composed document type.
    /// </summary>
    public required ReferenceByIdModel DocumentType { get; init; }

    /// <summary>
    /// Gets or sets the type of composition applied to the document type, indicating how this document type is composed (e.g., by content type, media type, etc.).
    /// </summary>
    public required CompositionType CompositionType { get; init; }
}
