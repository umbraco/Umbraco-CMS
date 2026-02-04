namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents the coordinates of an image crop area in the Delivery API.
/// </summary>
public class ImageCropCoordinates
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageCropCoordinates" /> class.
    /// </summary>
    /// <param name="x1">The X coordinate of the top-left corner.</param>
    /// <param name="y1">The Y coordinate of the top-left corner.</param>
    /// <param name="x2">The X coordinate of the bottom-right corner.</param>
    /// <param name="y2">The Y coordinate of the bottom-right corner.</param>
    public ImageCropCoordinates(decimal x1, decimal y1, decimal x2, decimal y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    /// <summary>
    ///     Gets the X coordinate of the top-left corner.
    /// </summary>
    public decimal X1 { get; }

    /// <summary>
    ///     Gets the Y coordinate of the top-left corner.
    /// </summary>
    public decimal Y1 { get; }

    /// <summary>
    ///     Gets the X coordinate of the bottom-right corner.
    /// </summary>
    public decimal X2 { get; }

    /// <summary>
    ///     Gets the Y coordinate of the bottom-right corner.
    /// </summary>
    public decimal Y2 { get; }
}
