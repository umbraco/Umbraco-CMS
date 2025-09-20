using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiMediaWithCropsBuilder : ApiMediaWithCropsBuilderBase<IApiMediaWithCrops>, IApiMediaWithCropsBuilder
{
    public ApiMediaWithCropsBuilder(IApiMediaBuilder apiMediaBuilder, IPublishedValueFallback publishedValueFallback)
        : base(apiMediaBuilder, publishedValueFallback)
    {
    }

    protected override IApiMediaWithCrops Create(
        IPublishedContent media,
        IApiMedia inner,
        ImageFocalPoint? focalPoint,
        IEnumerable<ImageCrop>? crops)
        => new ApiMediaWithCrops(inner, focalPoint, crops);
}
