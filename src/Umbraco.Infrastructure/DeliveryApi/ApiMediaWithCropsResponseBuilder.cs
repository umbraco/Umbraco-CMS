using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiMediaWithCropsResponseBuilder : ApiMediaWithCropsBuilderBase<IApiMediaWithCropsResponse>, IApiMediaWithCropsResponseBuilder
{
    public ApiMediaWithCropsResponseBuilder(IApiMediaBuilder apiMediaBuilder, IPublishedValueFallback publishedValueFallback)
        : base(apiMediaBuilder, publishedValueFallback)
    {
    }

    protected override IApiMediaWithCropsResponse Create(
        IPublishedContent media,
        IApiMedia inner,
        ImageFocalPoint? focalPoint,
        IEnumerable<ImageCrop>? crops)
    {
        var path = $"/{string.Join("/", PathSegments(media).Reverse())}/";
        return new ApiMediaWithCropsResponse(inner, focalPoint, crops, path, media.CreateDate, media.UpdateDate);
    }

    private IEnumerable<string> PathSegments(IPublishedContent media)
    {
        IPublishedContent? current = media;
        while (current != null)
        {
            yield return current.Name.ToLowerInvariant();
            current = current.Parent;
        }
    }
}
