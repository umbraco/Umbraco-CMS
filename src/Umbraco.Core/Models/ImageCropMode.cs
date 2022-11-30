namespace Umbraco.Cms.Core.Models;

public enum ImageCropMode
{
    /// <summary>
    ///     Resizes the image to the given dimensions. If the set dimensions do not match the aspect ratio of the original
    ///     image then the output is cropped to match the new aspect ratio.
    /// </summary>
    Crop,

    /// <summary>
    ///     Resizes the image to the given dimensions. If the set dimensions do not match the aspect ratio of the original
    ///     image then the output is resized to the maximum possible value in each direction while maintaining the original
    ///     aspect ratio.
    /// </summary>
    Max,

    /// <summary>
    ///     Resizes the image to the given dimensions. If the set dimensions do not match the aspect ratio of the original
    ///     image then the output is stretched to match the new aspect ratio.
    /// </summary>
    Stretch,

    /// <summary>
    ///     Passing a single dimension will automatically preserve the aspect ratio of the original image. If the requested
    ///     aspect ratio is different then the image will be padded to fit.
    /// </summary>
    Pad,

    /// <summary>
    ///     When upscaling an image the image pixels themselves are not resized, rather the image is padded to fit the given
    ///     dimensions.
    /// </summary>
    BoxPad,

    /// <summary>
    ///     Resizes the image until the shortest side reaches the set given dimension. This will maintain the aspect ratio of
    ///     the original image. Upscaling is disabled in this mode and the original image will be returned if attempted.
    /// </summary>
    Min,
}
