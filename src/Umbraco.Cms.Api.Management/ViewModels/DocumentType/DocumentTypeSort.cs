namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents information used to sort document types.
/// </summary>
public class DocumentTypeSort
{
    /// <summary>
    /// Gets or sets a reference to the document type identified by its ID.
    /// </summary>
    public required ReferenceByIdModel DocumentType { get; init; }

    /// <summary>
    /// Gets the sort order of the document type.
    /// </summary>
    public int SortOrder { get; init; }
}
