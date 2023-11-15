using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

[ShortGenericSchemaName<MediaValueModel, MediaVariantRequestModel>("CreateContentForMediaRequestModel")]
public class CreateMediaRequestModel : CreateContentRequestModelBase<MediaValueModel, MediaVariantRequestModel>
{
    public Guid ContentTypeId { get; set; }
}
