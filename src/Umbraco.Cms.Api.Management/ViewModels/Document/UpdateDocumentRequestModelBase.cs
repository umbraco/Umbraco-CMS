using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

    /// <summary>
    /// Serves as the base class for request models used to update documents in the API, providing support for value and variant models.
    /// </summary>
public abstract class UpdateDocumentRequestModelBase<TValueModel, TVariantModel> : UpdateContentRequestModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
}
