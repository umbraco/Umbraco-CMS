using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiMediaWithCropsBuilder : ApiMediaWithCropsBuilderBase<IApiMediaWithCrops>, IApiMediaWithCropsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiMediaWithCropsBuilder"/> class, which is responsible for building API media objects with crop data.
    /// </summary>
    /// <param name="apiMediaBuilder">An instance of <see cref="IApiMediaBuilder"/> used to construct API media representations.</param>
    /// <param name="publishedValueFallback">An instance of <see cref="IPublishedValueFallback"/> used to provide fallback values for published content properties.</param>
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
