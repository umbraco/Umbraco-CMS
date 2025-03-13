using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Extensions;

public static class DeliveryApiImageExtensions
{
    public static ImageFocalPoint? GetImageFocalPoint(this ImageCropperValue imageCropperValue)
        => imageCropperValue.FocalPoint is not null
            ? new ImageFocalPoint(imageCropperValue.FocalPoint.Left, imageCropperValue.FocalPoint.Top)
            : null;

    public static IEnumerable<ImageCrop>? GetImageCrops(this ImageCropperValue imageCropperValue)
        => imageCropperValue.Crops?.Select(crop => new ImageCrop(
            crop.Alias,
            crop.Width,
            crop.Height,
            crop.Coordinates is not null
                ? new ImageCropCoordinates(crop.Coordinates.X1, crop.Coordinates.Y1, crop.Coordinates.X2, crop.Coordinates.Y2)
                : null));
}
