using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.Models.ContentApi;

public class ApiMediaWithCrops : ApiMedia
{
    public ApiMediaWithCrops(Guid id, string name, string mediaType, string url, string? extension, int? width, int? height, IDictionary<string, object?> properties)
        : base(id, name, mediaType, url, extension, width, height, properties)
    {
    }

    public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; set; }

    public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; set; }
}
