namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents an image crop definition in the Delivery API.
/// </summary>
public class ImageCrop
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageCrop" /> class.
    /// </summary>
    /// <param name="alias">The alias of the crop.</param>
    /// <param name="width">The width of the crop in pixels.</param>
    /// <param name="height">The height of the crop in pixels.</param>
    /// <param name="coordinates">The coordinates of the crop area.</param>
    public ImageCrop(string? alias, int width, int height, ImageCropCoordinates? coordinates)
    {
        Alias = alias;
        Width = width;
        Height = height;
        Coordinates = coordinates;
    }

    /// <summary>
    ///     Gets the alias of the crop.
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    ///     Gets the width of the crop in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    ///     Gets the height of the crop in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    ///     Gets the coordinates of the crop area.
    /// </summary>
    public ImageCropCoordinates? Coordinates { get; }
}
