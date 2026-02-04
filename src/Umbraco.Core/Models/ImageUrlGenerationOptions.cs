namespace Umbraco.Cms.Core.Models;

/// <summary>
///     These are options that are passed to the IImageUrlGenerator implementation to determine the URL that is generated.
/// </summary>
public class ImageUrlGenerationOptions : IEquatable<ImageUrlGenerationOptions>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageUrlGenerationOptions" /> class.
    /// </summary>
    /// <param name="imageUrl">The base image URL.</param>
    public ImageUrlGenerationOptions(string? imageUrl) => ImageUrl = imageUrl;

    /// <summary>
    ///     Gets the base image URL.
    /// </summary>
    public string? ImageUrl { get; }

    /// <summary>
    ///     Gets or sets the source width of the original image in pixels.
    /// </summary>
    public int? SourceWidth { get; set; }

    /// <summary>
    ///     Gets or sets the source height of the original image in pixels.
    /// </summary>
    public int? SourceHeight { get; set; }

    /// <summary>
    ///     Gets or sets the desired output width in pixels.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    ///     Gets or sets the desired output height in pixels.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    ///     Gets or sets the image quality (typically 0-100).
    /// </summary>
    public int? Quality { get; set; }


    /// <summary>
    ///     Gets or sets the image format to use (for example "webp").
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    ///     Gets or sets the image crop mode to use.
    /// </summary>
    public ImageCropMode? ImageCropMode { get; set; }

    /// <summary>
    ///     Gets or sets the image crop anchor position.
    /// </summary>
    public ImageCropAnchor? ImageCropAnchor { get; set; }

    /// <summary>
    ///     Gets or sets the focal point position for cropping.
    /// </summary>
    public FocalPointPosition? FocalPoint { get; set; }

    /// <summary>
    ///     Gets or sets the crop coordinates.
    /// </summary>
    public CropCoordinates? Crop { get; set; }

    /// <summary>
    ///     Gets or sets a cache buster value to append to the URL.
    /// </summary>
    public string? CacheBusterValue { get; set; }

    /// <summary>
    ///     Gets or sets additional options to append to the generated URL.
    /// </summary>
    public string? FurtherOptions { get; set; }

    /// <inheritdoc />
    public bool Equals(ImageUrlGenerationOptions? other)
        => other != null &&
           ImageUrl == other.ImageUrl &&
           Width == other.Width &&
           Height == other.Height &&
           Quality == other.Quality &&
           Format == other.Format &&
           ImageCropMode == other.ImageCropMode &&
           ImageCropAnchor == other.ImageCropAnchor &&
           EqualityComparer<FocalPointPosition>.Default.Equals(FocalPoint, other.FocalPoint) &&
           EqualityComparer<CropCoordinates>.Default.Equals(Crop, other.Crop) &&
           CacheBusterValue == other.CacheBusterValue &&
           FurtherOptions == other.FurtherOptions;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ImageUrlGenerationOptions);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = default(HashCode);

        hash.Add(ImageUrl);
        hash.Add(Width);
        hash.Add(Height);
        hash.Add(Quality);
        hash.Add(Format);
        hash.Add(ImageCropMode);
        hash.Add(ImageCropAnchor);
        hash.Add(FocalPoint);
        hash.Add(Crop);
        hash.Add(CacheBusterValue);
        hash.Add(FurtherOptions);

        return hash.ToHashCode();
    }

    /// <summary>
    ///     The focal point position, in whatever units the registered IImageUrlGenerator uses, typically a percentage of the
    ///     total image from 0.0 to 1.0.
    /// </summary>
    public class FocalPointPosition : IEquatable<FocalPointPosition>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FocalPointPosition" /> class.
        /// </summary>
        /// <param name="left">The left position as a percentage (0.0 to 1.0).</param>
        /// <param name="top">The top position as a percentage (0.0 to 1.0).</param>
        public FocalPointPosition(decimal left, decimal top)
        {
            Left = left;
            Top = top;
        }

        /// <summary>
        ///     Gets the left position of the focal point as a percentage of the total image width.
        /// </summary>
        public decimal Left { get; }

        /// <summary>
        ///     Gets the top position of the focal point as a percentage of the total image height.
        /// </summary>
        public decimal Top { get; }

        /// <inheritdoc />
        public bool Equals(FocalPointPosition? other)
            => other != null &&
               Left == other.Left &&
               Top == other.Top;

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as FocalPointPosition);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Left, Top);
    }

    /// <summary>
    ///     The bounds of the crop within the original image, in whatever units the registered IImageUrlGenerator uses,
    ///     typically a percentage between 0.0 and 1.0.
    /// </summary>
    public class CropCoordinates : IEquatable<CropCoordinates>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CropCoordinates" /> class.
        /// </summary>
        /// <param name="left">The left coordinate as a percentage (0.0 to 1.0).</param>
        /// <param name="top">The top coordinate as a percentage (0.0 to 1.0).</param>
        /// <param name="right">The right coordinate as a percentage (0.0 to 1.0).</param>
        /// <param name="bottom">The bottom coordinate as a percentage (0.0 to 1.0).</param>
        public CropCoordinates(decimal left, decimal top, decimal right, decimal bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        ///     Gets the left coordinate of the crop as a percentage of the total image width.
        /// </summary>
        public decimal Left { get; }

        /// <summary>
        ///     Gets the top coordinate of the crop as a percentage of the total image height.
        /// </summary>
        public decimal Top { get; }

        /// <summary>
        ///     Gets the right coordinate of the crop as a percentage of the total image width.
        /// </summary>
        public decimal Right { get; }

        /// <summary>
        ///     Gets the bottom coordinate of the crop as a percentage of the total image height.
        /// </summary>
        public decimal Bottom { get; }

        /// <inheritdoc />
        public bool Equals(CropCoordinates? other)
            => other != null &&
               Left == other.Left &&
               Top == other.Top &&
               Right == other.Right &&
               Bottom == other.Bottom;

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as CropCoordinates);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);
    }
}
