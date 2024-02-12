using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class CreateContentWithParentRequestModelBase<TValueModel, TVariantModel>
    : CreateContentRequestModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    public ReferenceByIdModel? Parent { get; set; }
}
