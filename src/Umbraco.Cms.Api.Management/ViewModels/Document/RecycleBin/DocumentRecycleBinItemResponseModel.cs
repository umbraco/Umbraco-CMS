using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.RecycleBin;

/// <summary>
/// Represents the response model for a single document item contained within the recycle bin.
/// Used to transfer information about deleted documents in API responses.
/// </summary>
public class DocumentRecycleBinItemResponseModel : RecycleBinItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the document type reference for the recycle bin item.
    /// </summary>
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variant items associated with the document in the recycle bin.
    /// </summary>
    public IEnumerable<DocumentVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<DocumentVariantItemResponseModel>();
}
