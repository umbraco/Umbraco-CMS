using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiMediaWithCropsResponse : ApiMediaWithCrops
{
    public ApiMediaWithCropsResponse(IApiMedia inner, ImageCropperValue.ImageCropperFocalPoint? focalPoint, IEnumerable<ImageCropperValue.ImageCropperCrop>? crops, string path)
        : base(inner, focalPoint, crops)
        => Path = path;

    public string Path { get; }
}
