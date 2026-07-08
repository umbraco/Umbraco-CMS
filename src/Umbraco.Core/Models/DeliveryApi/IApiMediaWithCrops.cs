namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents media with image cropping information in the Delivery API.
/// </summary>
public interface IApiMediaWithCrops : IApiMedia
{
    /// <summary>
    ///     Gets the focal point of the image.
    /// </summary>
    ImageFocalPoint? FocalPoint { get; }

    /// <summary>
    ///     Gets the defined image crops.
    /// </summary>
    IEnumerable<ImageCrop>? Crops { get; }

    /// <summary>
    ///     Gets the alternative text for this media item.
    /// </summary>
    // TODO (V20): Remove the default implementation once all external implementations have been updated.
    string? AltText => null;
}
