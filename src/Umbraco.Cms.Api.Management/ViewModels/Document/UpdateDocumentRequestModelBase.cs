using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public abstract class UpdateDocumentRequestModelBase<TValueModel, TVariantModel> : UpdateContentRequestModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
}
