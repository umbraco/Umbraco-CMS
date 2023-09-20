using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiMediaWithCropsResponseBuilder : ApiMediaWithCropsBuilderBase<ApiMediaWithCropsResponse>, IApiMediaWithCropsResponseBuilder
{
    public ApiMediaWithCropsResponseBuilder(IApiMediaBuilder apiMediaBuilder, IPublishedValueFallback publishedValueFallback)
        : base(apiMediaBuilder, publishedValueFallback)
    {
    }

    protected override ApiMediaWithCropsResponse Create(
        IPublishedContent media,
        IApiMedia inner,
        ImageCropperValue.ImageCropperFocalPoint? focalPoint,
        IEnumerable<ImageCropperValue.ImageCropperCrop>? crops)
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
