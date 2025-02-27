using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public abstract class CreateDocumentRequestModelBase<TValueModel, TVariantModel>
    : CreateContentWithParentRequestModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    public required ReferenceByIdModel DocumentType { get; set; }
}
