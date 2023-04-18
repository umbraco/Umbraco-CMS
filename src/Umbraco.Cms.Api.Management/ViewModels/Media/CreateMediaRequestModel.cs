using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class CreateMediaRequestModel : CreateContentRequestModelBase<MediaValueModel, MediaVariantRequestModel>
{
    public Guid ContentTypeId { get; set; }
}
