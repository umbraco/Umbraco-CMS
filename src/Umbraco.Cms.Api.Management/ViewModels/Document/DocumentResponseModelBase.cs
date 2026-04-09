using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Serves as the base response model for document entities, parameterized by the value and variant response model types.
/// </summary>
public abstract class DocumentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    /// <summary>
    /// Gets or sets a reference to the document type associated with this document.
    /// </summary>
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();
}
