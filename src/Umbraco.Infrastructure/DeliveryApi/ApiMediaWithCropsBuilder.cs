using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiMediaWithCropsBuilder : IApiMediaWithCropsBuilder
{
    private readonly IApiMediaBuilder _apiMediaBuilder;
    private readonly IPublishedValueFallback _publishedValueFallback;

    public ApiMediaWithCropsBuilder(IApiMediaBuilder apiMediaBuilder, IPublishedValueFallback publishedValueFallback)
    {
        _apiMediaBuilder = apiMediaBuilder;
        _publishedValueFallback = publishedValueFallback;
    }

    public ApiMediaWithCrops Build(MediaWithCrops media)
    {
        IApiMedia inner = _apiMediaBuilder.Build(media.Content);

        // make sure we merge crops and focal point defined at media level with the locally defined ones (local ones take precedence in case of a conflict)
        ImageCropperValue? mediaCrops = media.Content.Value<ImageCropperValue>(_publishedValueFallback, Constants.Conventions.Media.File);
        ImageCropperValue localCrops = media.LocalCrops;
        if (mediaCrops != null)
        {
            localCrops = localCrops.Merge(mediaCrops);
        }

        return new ApiMediaWithCrops(inner, localCrops.FocalPoint, localCrops.Crops);
    }

    public ApiMediaWithCrops Build(IPublishedContent media)
    {
        var mediaWithCrops = new MediaWithCrops(media, _publishedValueFallback, new ImageCropperValue());
        return Build(mediaWithCrops);
    }
}
