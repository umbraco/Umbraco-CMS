using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.Headless;

public class HeadlessMediaWithCrops : HeadlessMedia
{
    public HeadlessMediaWithCrops(Guid key, string? name, string mediaType, string url, string? extension, int? width, int? height, IDictionary<string, object?> properties)
        : base(key, name, mediaType, url, extension, width, height, properties)
    {
    }

    public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; set; }

    public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; set; }
}
