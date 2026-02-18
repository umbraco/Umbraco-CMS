namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents media with image cropping information in the Delivery API.
/// </summary>
public interface IApiMediaWithCrops : IApiMedia
{
    /// <summary>
    ///     Gets the focal point of the image.
    /// </summary>
    public ImageFocalPoint? FocalPoint { get; }

    /// <summary>
    ///     Gets the defined image crops.
    /// </summary>
    public IEnumerable<ImageCrop>? Crops { get; }
}
