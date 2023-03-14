namespace Umbraco.Cms.Core.Models;

/// <summary>
///     These are options that are passed to the IImageUrlGenerator implementation to determine the URL that is generated.
/// </summary>
public class ImageUrlGenerationOptions : IEquatable<ImageUrlGenerationOptions>
{
    public ImageUrlGenerationOptions(string? imageUrl) => ImageUrl = imageUrl;

    public string? ImageUrl { get; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public int? Quality { get; set; }

    public ImageCropMode? ImageCropMode { get; set; }

    public ImageCropAnchor? ImageCropAnchor { get; set; }

    public FocalPointPosition? FocalPoint { get; set; }

    public CropCoordinates? Crop { get; set; }

    public string? CacheBusterValue { get; set; }

    public string? FurtherOptions { get; set; }

    public bool Equals(ImageUrlGenerationOptions? other)
        => other != null &&
           ImageUrl == other.ImageUrl &&
           Width == other.Width &&
           Height == other.Height &&
           Quality == other.Quality &&
           ImageCropMode == other.ImageCropMode &&
           ImageCropAnchor == other.ImageCropAnchor &&
           EqualityComparer<FocalPointPosition>.Default.Equals(FocalPoint, other.FocalPoint) &&
           EqualityComparer<CropCoordinates>.Default.Equals(Crop, other.Crop) &&
           CacheBusterValue == other.CacheBusterValue &&
           FurtherOptions == other.FurtherOptions;

    public override bool Equals(object? obj) => Equals(obj as ImageUrlGenerationOptions);

    public override int GetHashCode()
    {
        var hash = default(HashCode);

        hash.Add(ImageUrl);
        hash.Add(Width);
        hash.Add(Height);
        hash.Add(Quality);
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
        public FocalPointPosition(decimal left, decimal top)
        {
            Left = left;
            Top = top;
        }

        public decimal Left { get; }

        public decimal Top { get; }

        public bool Equals(FocalPointPosition? other)
            => other != null &&
               Left == other.Left &&
               Top == other.Top;

        public override bool Equals(object? obj) => Equals(obj as FocalPointPosition);

        public override int GetHashCode() => HashCode.Combine(Left, Top);
    }

    /// <summary>
    ///     The bounds of the crop within the original image, in whatever units the registered IImageUrlGenerator uses,
    ///     typically a percentage between 0.0 and 1.0.
    /// </summary>
    public class CropCoordinates : IEquatable<CropCoordinates>
    {
        public CropCoordinates(decimal left, decimal top, decimal right, decimal bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public decimal Left { get; }

        public decimal Top { get; }

        public decimal Right { get; }

        public decimal Bottom { get; }

        public bool Equals(CropCoordinates? other)
            => other != null &&
               Left == other.Left &&
               Top == other.Top &&
               Right == other.Right &&
               Bottom == other.Bottom;

        public override bool Equals(object? obj) => Equals(obj as CropCoordinates);

        public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);
    }
}
