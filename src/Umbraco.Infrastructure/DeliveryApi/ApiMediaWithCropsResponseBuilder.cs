using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiMediaWithCropsResponseBuilder : ApiMediaWithCropsBuilderBase<IApiMediaWithCropsResponse>, IApiMediaWithCropsResponseBuilder
{
    private readonly IPublishedMediaCache _mediaCache;
    private readonly IMediaNavigationQueryService _navigationQueryService;

    public ApiMediaWithCropsResponseBuilder(
        IApiMediaBuilder apiMediaBuilder,
        IPublishedValueFallback publishedValueFallback,
        IPublishedMediaCache mediaCache,
        IMediaNavigationQueryService navigationQueryService)
        : base(apiMediaBuilder, publishedValueFallback)
    {
        _mediaCache = mediaCache;
        _navigationQueryService = navigationQueryService;
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
            current = GetParent(current);
        }
    }

    private IPublishedContent? GetParent(IPublishedContent media)
    {
        IPublishedContent? parent;
        if (_navigationQueryService.TryGetParentKey(media.Key, out Guid? parentKey))
        {
            parent = parentKey.HasValue ? _mediaCache.GetById(parentKey.Value) : null;
        }
        else
        {
            throw new KeyNotFoundException($"Media with key '{media.Key}' was not found in the in-memory navigation structure.");
        }

        return parent;
    }
}
