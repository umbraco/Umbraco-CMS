using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Serves as the base response model for element entities, parameterized by the value and variant response model types.
/// </summary>
/// <typeparam name="TValueResponseModelBase">The type of the value response model, derived from <see cref="ValueModelBase"/>.</typeparam>
/// <typeparam name="TVariantResponseModel">The type of the variant response model, derived from <see cref="VariantResponseModelBase"/>.</typeparam>
public abstract class ElementResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    /// <summary>
    /// Gets or sets a reference to the document type associated with this element.
    /// </summary>
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();
}
