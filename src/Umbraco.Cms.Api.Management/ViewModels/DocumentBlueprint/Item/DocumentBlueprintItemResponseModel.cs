using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;

/// <summary>
/// Represents a response model for an individual document blueprint item in the API management context.
/// </summary>
public class DocumentBlueprintItemResponseModel : NamedItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the document type reference associated with the document blueprint item.
    /// </summary>
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();
}
