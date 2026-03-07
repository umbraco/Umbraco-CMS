using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

    /// <summary>
    /// Serves as a base class for request models used when creating documents in Umbraco, allowing specification of custom value and variant models.
    /// </summary>
    /// <typeparam name="TValueModel">The type representing the values associated with the document.</typeparam>
    /// <typeparam name="TVariantModel">The type representing the variants (such as language or culture variants) of the document.</typeparam>
public abstract class CreateDocumentRequestModelBase<TValueModel, TVariantModel>
    : CreateContentWithParentRequestModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    /// <summary>
    /// Gets or sets a reference to the document type, identified by its ID.
    /// </summary>
    public required ReferenceByIdModel DocumentType { get; set; }
}
