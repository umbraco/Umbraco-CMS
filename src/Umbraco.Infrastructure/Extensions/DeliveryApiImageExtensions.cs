using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for working with images in the Umbraco Delivery API.
/// </summary>
public static class DeliveryApiImageExtensions
{
    /// <summary>
    /// Gets the focal point of the specified <see cref="ImageCropperValue"/> if it exists.
    /// </summary>
    /// <param name="imageCropperValue">The image cropper value to extract the focal point from.</param>
    /// <returns>The <see cref="ImageFocalPoint"/> if a focal point is defined; otherwise, <c>null</c>.</returns>
    public static ImageFocalPoint? GetImageFocalPoint(this ImageCropperValue imageCropperValue)
        => imageCropperValue.FocalPoint is not null
            ? new ImageFocalPoint(imageCropperValue.FocalPoint.Left, imageCropperValue.FocalPoint.Top)
            : null;

    /// <summary>
    /// Returns a collection of <see cref="ImageCrop"/> objects extracted from the specified <see cref="ImageCropperValue"/>.
    /// </summary>
    /// <param name="imageCropperValue">The <see cref="ImageCropperValue"/> instance from which to extract image crops.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="ImageCrop"/> representing the crops defined in the image cropper value, or <c>null</c> if no crops are present.
    /// </returns>
    public static IEnumerable<ImageCrop>? GetImageCrops(this ImageCropperValue imageCropperValue)
        => imageCropperValue.Crops?.Select(crop => new ImageCrop(
            crop.Alias,
            crop.Width,
            crop.Height,
            crop.Coordinates is not null
                ? new ImageCropCoordinates(crop.Coordinates.X1, crop.Coordinates.Y1, crop.Coordinates.X2, crop.Coordinates.Y2)
                : null));
}
