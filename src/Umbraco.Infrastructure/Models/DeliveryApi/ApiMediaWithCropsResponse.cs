using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiMediaWithCropsResponse : ApiMediaWithCrops
{
    public ApiMediaWithCropsResponse(
        IApiMedia inner,
        ImageCropperValue.ImageCropperFocalPoint? focalPoint,
        IEnumerable<ImageCropperValue.ImageCropperCrop>? crops,
        string path,
        DateTime createDate,
        DateTime updateDate)
        : base(inner, focalPoint, crops)
    {
        Path = path;
        CreateDate = createDate;
        UpdateDate = updateDate;
    }

    public string Path { get; }

    public DateTime CreateDate { get; }

    public DateTime UpdateDate { get; }
}
