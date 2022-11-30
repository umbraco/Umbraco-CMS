using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.Headless;

public class HeadlessImageCropperValue
{
    public HeadlessImageCropperValue(string url, ImageCropperValue.ImageCropperFocalPoint? focalPoint, IEnumerable<ImageCropperValue.ImageCropperCrop>? crops)
    {
        Url = url;
        FocalPoint = focalPoint;
        Crops = crops;
    }

    public string Url { get; }

    public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; }

    public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; }
}
