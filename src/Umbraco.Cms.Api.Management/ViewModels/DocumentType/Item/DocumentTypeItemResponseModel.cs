using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;

/// <summary>
/// Represents a response model containing information about a document type item returned by the Umbraco Management API.
/// </summary>
public class DocumentTypeItemResponseModel : NamedItemResponseModelBase
{
    /// <summary>
    /// Gets or sets a value indicating whether this document type is an element.
    /// </summary>
    public bool IsElement { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the document type.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// The description of the document type.
    /// </summary>
    public string? Description { get; set; }
}
