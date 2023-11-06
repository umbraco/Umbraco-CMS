using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

[ShortGenericSchemaName<MediaValueModel, MediaVariantRequestModel>("UpdateContentForMediaRequestModel")]
public class UpdateMediaRequestModel : UpdateContentRequestModelBase<MediaValueModel, MediaVariantRequestModel>
{
}
