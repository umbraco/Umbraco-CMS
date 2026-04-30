namespace Umbraco.Cms.Core.Models.DeliveryApi;

internal sealed class ApiImageCropperValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Models.DeliveryApi.ApiImageCropperValue"/> class with the specified image URL, focal point, and optional image crops.
    /// </summary>
    /// <param name="url">The URL of the image.</param>
    /// <param name="focalPoint">The focal point of the image, or <c>null</c> if not specified.</param>
    /// <param name="crops">A collection of image crops, or <c>null</c> if none are provided.</param>
    public ApiImageCropperValue(string url, ImageFocalPoint? focalPoint, IEnumerable<ImageCrop>? crops)
    {
        Url = url;
        FocalPoint = focalPoint;
        Crops = crops;
    }

    /// <summary>
    /// Gets the URL for the cropped image represented by this value.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Gets the focal point coordinates of the image cropper value, if a focal point has been set.
    /// Returns <c>null</c> if no focal point is defined.
    /// </summary>
    public ImageFocalPoint? FocalPoint { get; }

    /// <summary>
    /// Gets the collection of image crops associated with this image cropper value.
    /// </summary>
    public IEnumerable<ImageCrop>? Crops { get; }
}
