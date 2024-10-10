using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class CreateMediaRequestModel : CreateContentWithParentRequestModelBase<MediaValueModel, MediaVariantRequestModel>
{
    public required ReferenceByIdModel MediaType { get; set; }
}
