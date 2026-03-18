namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents the response model containing allowed parent document types for a document type.
/// </summary>
public class DocumentTypeAllowedParentsResponseModel
{
    /// <summary>
    /// Gets or sets the collection of parent document type references that are allowed for this document type.
    /// </summary>
    public required ISet<ReferenceByIdModel> AllowedParentIds { get; set; }
}
