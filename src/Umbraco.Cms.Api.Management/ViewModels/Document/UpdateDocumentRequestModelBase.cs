using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

[ShortGenericSchemaName<DocumentValueModel, DocumentVariantRequestModel>("UpdateContentForDocumentRequestModel")]
public abstract class UpdateDocumentRequestModelBase<TValueModel, TVariantModel> : UpdateContentRequestModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
}
