using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

namespace Umbraco.Cms.Api.Management.ViewModels.Element.RecycleBin;

/// <summary>
/// Represents the response model for a single element item contained within the recycle bin.
/// Used to transfer information about deleted elements in API responses.
/// </summary>
public class ElementRecycleBinItemResponseModel : RecycleBinItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the document type reference for the recycle bin item.
    /// </summary>
    public DocumentTypeReferenceResponseModel? DocumentType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variant items associated with the element in the recycle bin.
    /// </summary>
    public IEnumerable<ElementVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<ElementVariantItemResponseModel>();

    /// <summary>
    /// Gets or sets a value indicating whether this recycle bin item is a folder.
    /// </summary>
    public bool IsFolder { get; set; }

    /// <summary>
    /// Gets or sets the name of the element in the recycle bin.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
