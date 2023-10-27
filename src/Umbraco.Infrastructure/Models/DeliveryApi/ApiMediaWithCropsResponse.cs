namespace Umbraco.Cms.Core.Models.DeliveryApi;

internal sealed class ApiMediaWithCropsResponse : ApiMediaWithCrops, IApiMediaWithCropsResponse
{
    public ApiMediaWithCropsResponse(
        IApiMedia inner,
        ImageFocalPoint? focalPoint,
        IEnumerable<ImageCrop>? crops,
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
