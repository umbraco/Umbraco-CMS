using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public class CreateElementRequestModel : CreateContentWithParentRequestModelBase<ElementValueModel, ElementVariantRequestModel>
{
    public required ReferenceByIdModel DocumentType { get; set; }
}
