namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents a request model used by the Management API for importing a document type, typically containing the data required for the import operation.
/// </summary>
public class ImportDocumentTypeRequestModel
{
    /// <summary>
    /// Gets or sets a reference to the file containing the document type definition to import.
    /// </summary>
    public required ReferenceByIdModel File { get; set; }
}
