using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiMediaWithCropsBuilder : ApiMediaWithCropsBuilderBase<ApiMediaWithCrops>, IApiMediaWithCropsBuilder
{
    public ApiMediaWithCropsBuilder(IApiMediaBuilder apiMediaBuilder, IPublishedValueFallback publishedValueFallback)
        : base(apiMediaBuilder, publishedValueFallback)
    {
    }

    protected override ApiMediaWithCrops Create(
        IPublishedContent media,
        IApiMedia inner,
        ImageCropperValue.ImageCropperFocalPoint? focalPoint,
        IEnumerable<ImageCropperValue.ImageCropperCrop>? crops) =>
        new ApiMediaWithCrops(inner, focalPoint, crops);
}
