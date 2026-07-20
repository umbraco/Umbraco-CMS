namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents the focal point of an image in the Delivery API.
/// </summary>
public class ImageFocalPoint
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageFocalPoint" /> class.
    /// </summary>
    /// <param name="left">The horizontal position of the focal point (0-1, where 0 is left edge and 1 is right edge).</param>
    /// <param name="top">The vertical position of the focal point (0-1, where 0 is top edge and 1 is bottom edge).</param>
    public ImageFocalPoint(decimal left, decimal top)
    {
        Left = left;
        Top = top;
    }

    /// <summary>
    ///     Gets the horizontal position of the focal point.
    /// </summary>
    /// <remarks>
    ///     Value ranges from 0 (left edge) to 1 (right edge).
    /// </remarks>
    public decimal Left { get; }

    /// <summary>
    ///     Gets the vertical position of the focal point.
    /// </summary>
    /// <remarks>
    ///     Value ranges from 0 (top edge) to 1 (bottom edge).
    /// </remarks>
    public decimal Top { get; }
}
