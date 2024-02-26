namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiMediaWithCrops : IApiMedia
{
    public ImageFocalPoint? FocalPoint { get; }

    public IEnumerable<ImageCrop>? Crops { get; }
}
