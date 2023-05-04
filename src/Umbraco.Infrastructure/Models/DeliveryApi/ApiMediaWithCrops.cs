using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiMediaWithCrops : IApiMedia
{
    private readonly IApiMedia _inner;

    public ApiMediaWithCrops(
        IApiMedia inner,
        ImageCropperValue.ImageCropperFocalPoint? focalPoint,
        IEnumerable<ImageCropperValue.ImageCropperCrop>? crops)
    {
        _inner = inner;
        FocalPoint = focalPoint;
        Crops = crops;
    }

    public Guid Id => _inner.Id;

    public string Name => _inner.Name;

    public string MediaType => _inner.MediaType;

    public string Url => _inner.Url;

    public IDictionary<string, object?> Properties => _inner.Properties;

    public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; }

    public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; }
}
