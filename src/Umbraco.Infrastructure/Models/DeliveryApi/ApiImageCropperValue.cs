namespace Umbraco.Cms.Core.Models.DeliveryApi;

internal sealed class ApiImageCropperValue
{
    public ApiImageCropperValue(string url, ImageFocalPoint? focalPoint, IEnumerable<ImageCrop>? crops)
    {
        Url = url;
        FocalPoint = focalPoint;
        Crops = crops;
    }

    public string Url { get; }

    public ImageFocalPoint? FocalPoint { get; }

    public IEnumerable<ImageCrop>? Crops { get; }
}
