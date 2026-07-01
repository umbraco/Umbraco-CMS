// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Represents a value of the image cropper value editor.
/// </summary>
public class ImageCropperValue :TemporaryFileUploadValueBase, IHtmlEncodedString, IEquatable<ImageCropperValue>
{
    /// <summary>
    ///     Gets or sets the value focal point.
    /// </summary>
    public ImageCropperFocalPoint? FocalPoint { get; set; }

    /// <summary>
    ///     Gets or sets the value crops.
    /// </summary>
    public IEnumerable<ImageCropperCrop>? Crops { get; set; }

    /// <inheritdoc />
    public string? ToHtmlString() => Src;

    /// <summary>
    /// Returns the image source URL represented by this <see cref="ImageCropperValue"/>.
    /// </summary>
    /// <returns>The image source URL if available; otherwise, <c>null</c>.</returns>
    public override string? ToString() => Src;

    /// <summary>
    ///     Gets a crop.
    /// </summary>
    public ImageCropperCrop? GetCrop(string? alias)
    {
        if (Crops == null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(alias)
            ? Crops.FirstOrDefault()
            : Crops.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
    }

    /// <summary>
    /// Creates and returns image URL generation options based on the provided image URL, crop information, and focal point preference.
    /// If <paramref name="preferFocalPoint"/> is true and a focal point is available, or if the crop is specified without coordinates and a focal point exists, the returned options will use the focal point.
    /// If crop coordinates are provided and <paramref name="preferFocalPoint"/> is false, the returned options will use the crop coordinates.
    /// Otherwise, default options are returned.
    /// </summary>
    /// <param name="url">The URL of the image.</param>
    /// <param name="crop">The crop information to apply to the image, or <c>null</c> if not specified.</param>
    /// <param name="preferFocalPoint">If <c>true</c>, prefer using the focal point over crop coordinates when both are available.</param>
    /// <returns>An <see cref="Umbraco.Cms.Core.PropertyEditors.ValueConverters.ImageUrlGenerationOptions"/> instance configured with either the focal point, crop coordinates, or default settings based on the input parameters.</returns>
    public ImageUrlGenerationOptions GetCropBaseOptions(string? url, ImageCropperCrop? crop, bool preferFocalPoint)
    {
        if ((preferFocalPoint && HasFocalPoint()) || (crop is not null && crop.Coordinates is null && HasFocalPoint()))
        {
            return new ImageUrlGenerationOptions(url)
            {
                FocalPoint = new ImageUrlGenerationOptions.FocalPointPosition(FocalPoint!.Left, FocalPoint.Top)
            };
        }

        if (crop is not null && crop.Coordinates is not null && preferFocalPoint == false)
        {
            return new ImageUrlGenerationOptions(url)
            {
                Crop = new ImageUrlGenerationOptions.CropCoordinates(
                    crop.Coordinates.X1,
                    crop.Coordinates.Y1,
                    crop.Coordinates.X2,
                    crop.Coordinates.Y2)
            };
        }

        return new ImageUrlGenerationOptions(url);
    }

    /// <summary>
    ///     Returns the image URL for the specified crop alias using the provided image URL generator.
    /// </summary>
    /// <param name="alias">The alias of the crop to retrieve the URL for. If empty or not found, the original image or focal point may be used.</param>
    /// <param name="imageUrlGenerator">The image URL generator used to construct the URL.</param>
    /// <param name="useCropDimensions">If true, the crop's width and height are used in the generated URL; otherwise, dimensions are not applied.</param>
    /// <param name="useFocalPoint">If true, the focal point is used in URL generation. If false, the crop's coordinates are used if available.</param>
    /// <param name="cacheBusterValue">An optional value appended to the URL to prevent caching issues.</param>
    /// <returns>
    ///   The URL of the cropped image if the crop is found; otherwise, null if the specified crop alias does not exist.
    /// </returns>
    public string? GetCropUrl(
        string alias,
        IImageUrlGenerator imageUrlGenerator,
        bool useCropDimensions = true,
        bool useFocalPoint = false,
        string? cacheBusterValue = null)
    {
        ImageCropperCrop? crop = GetCrop(alias);

        // could not find a crop with the specified, non-empty, alias
        if (crop is null && !string.IsNullOrWhiteSpace(alias))
        {
            return null;
        }

        ImageUrlGenerationOptions options =
            GetCropBaseOptions(Src, crop, useFocalPoint || string.IsNullOrWhiteSpace(alias));

        if (crop is not null && useCropDimensions)
        {
            options.Width = crop.Width;
            options.Height = crop.Height;
        }

        options.CacheBusterValue = cacheBusterValue;

        return imageUrlGenerator.GetImageUrl(options);
    }

    /// <summary>
    ///     Gets the value image URL for a specific width and height.
    /// </summary>
    public string? GetCropUrl(
        int width,
        int height,
        IImageUrlGenerator imageUrlGenerator,
        string? cacheBusterValue = null)
    {
        ImageUrlGenerationOptions options = GetCropBaseOptions(null, null, false);

        options.Width = width;
        options.Height = height;
        options.CacheBusterValue = cacheBusterValue;

        return imageUrlGenerator.GetImageUrl(options);
    }

    /// <summary>
    ///     Determines whether the value has a focal point.
    /// </summary>
    /// <returns></returns>
    public bool HasFocalPoint()
        => FocalPoint is not null && (FocalPoint.Left != 0.5m || FocalPoint.Top != 0.5m);

    /// <summary>
    ///     Determines whether the value has crops.
    /// </summary>
    /// <returns><c>true</c> if the value has crops; otherwise, <c>false</c>.</returns>
    public bool HasCrops()
        => Crops is not null && Crops.Any();

    /// <summary>
    /// Determines whether a crop with the specified alias exists in the value.
    /// </summary>
    /// <param name="alias">The alias of the crop to check for.</param>
    /// <returns><c>true</c> if a crop with the specified alias exists; otherwise, <c>false</c>.</returns>
    public bool HasCrop(string alias)
        => Crops is not null && Crops.Any(x => x.Alias == alias);

    /// <summary>
    /// Checks if the value contains a source image.
    /// </summary>
    /// <returns>Returns <c>true</c> if a source image is present; otherwise, <c>false</c>.</returns>
    public bool HasImage()
        => !string.IsNullOrWhiteSpace(Src);

    /// <summary>
    /// Merges the current <see cref="ImageCropperValue"/> instance with another, combining their crops and properties.
    /// Crops from the provided <paramref name="imageCropperValue"/> are added if they do not exist in the current instance, or their coordinates are used if missing in the current crop.
    /// The <c>Src</c> property is taken from the current instance if available; otherwise, it falls back to the provided value.
    /// The <c>FocalPoint</c> is taken from the current instance if set; otherwise, it uses the provided value.
    /// </summary>
    /// <param name="imageCropperValue">The <see cref="ImageCropperValue"/> to merge with the current instance.</param>
    /// <returns>A new <see cref="ImageCropperValue"/> instance containing the merged crops and properties.</returns>
    public ImageCropperValue Merge(ImageCropperValue imageCropperValue)
    {
        List<ImageCropperCrop> crops = Crops?.ToList() ?? new List<ImageCropperCrop>();

        IEnumerable<ImageCropperCrop>? incomingCrops = imageCropperValue.Crops;
        if (incomingCrops != null)
        {
            foreach (ImageCropperCrop incomingCrop in incomingCrops)
            {
                ImageCropperCrop? crop = crops.FirstOrDefault(x => x.Alias == incomingCrop.Alias);
                if (crop is null)
                {
                    // Add incoming crop
                    crops.Add(incomingCrop);
                }
                else if (crop.Coordinates is null)
                {
                    // Use incoming crop coordinates
                    crop.Coordinates = incomingCrop.Coordinates;
                }
            }
        }

        return new ImageCropperValue
        {
            Src = !string.IsNullOrWhiteSpace(Src) ? Src : imageCropperValue.Src,
            Crops = crops,
            FocalPoint = FocalPoint ?? imageCropperValue.FocalPoint
        };
    }

    /// <summary>
    ///     Removes redundant crop data/default focal point.
    /// </summary>
    internal void Prune()
    {
        Crops = Crops?.Where(crop => crop.Coordinates != null).ToArray();
        if (FocalPoint is { Top: 0.5m, Left: 0.5m })
        {
            FocalPoint = null;
        }
    }

    /// <summary>
    /// Represents the focal point coordinates (typically X and Y values) used by the image cropper to determine the main area of interest within an image.
    /// </summary>
    public class ImageCropperFocalPoint : IEquatable<ImageCropperFocalPoint>
    {
        /// <summary>
        /// Gets or sets the left coordinate of the focal point.
        /// </summary>
        public decimal Left { get; set; }

        /// <summary>
        /// Gets or sets the vertical (top) coordinate of the focal point within the image cropper.
        /// </summary>
        public decimal Top { get; set; }

        #region IEquatable

        /// <inheritdoc />
        public bool Equals(ImageCropperFocalPoint? other)
            => ReferenceEquals(this, other) || Equals(this, other);

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || (obj is ImageCropperFocalPoint other && Equals(this, other));

        private static bool Equals(ImageCropperFocalPoint left, ImageCropperFocalPoint? right)
            => ReferenceEquals(left, right) // deals with both being null, too
               || (!ReferenceEquals(left, null) && !ReferenceEquals(right, null)
                                                && left.Left == right.Left
                                                && left.Top == right.Top);

        public static bool operator ==(ImageCropperFocalPoint left, ImageCropperFocalPoint right)
            => Equals(left, right);

        public static bool operator !=(ImageCropperFocalPoint left, ImageCropperFocalPoint right)
            => !Equals(left, right);

        /// <summary>
        /// Returns a hash code for this instance, based on the <c>Left</c> and <c>Top</c> properties.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ImageCropperFocalPoint"/> object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // properties are, practically, readonly
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return (Left.GetHashCode() * 397) ^ Top.GetHashCode();
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a crop area defined within an image for the image cropper value.
    /// Contains information about the position and dimensions of the crop.
    /// </summary>
    public class ImageCropperCrop : IEquatable<ImageCropperCrop>
    {
        /// <summary>
        /// Gets or sets the alias of the image crop.
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Gets or sets the width of the image crop.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the image crop.
        /// </summary>
        public int Height { get; set; }

        /// <summary>Gets or sets the coordinates for the image crop.</summary>
        public ImageCropperCropCoordinates? Coordinates { get; set; }

        #region IEquatable

        /// <inheritdoc />
        public bool Equals(ImageCropperCrop? other)
            => ReferenceEquals(this, other) || Equals(this, other);

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || (obj is ImageCropperCrop other && Equals(this, other));

        private static bool Equals(ImageCropperCrop? left, ImageCropperCrop? right)
            => ReferenceEquals(left, right) // deals with both being null, too
               || (!ReferenceEquals(left, null) && !ReferenceEquals(right, null)
                                                && string.Equals(left.Alias, right.Alias)
                                                && left.Width == right.Width
                                                && left.Height == right.Height
                                                && Equals(left.Coordinates, right.Coordinates));

        public static bool operator ==(ImageCropperCrop? left, ImageCropperCrop? right)
            => Equals(left, right);

        public static bool operator !=(ImageCropperCrop? left, ImageCropperCrop? right)
            => !Equals(left, right);

        /// <summary>
        /// Returns a hash code for this <see cref="ImageCropperCrop"/> instance, based on its properties.
        /// </summary>
        /// <returns>A hash code representing the current crop's state.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // properties are, practically, readonly
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = Alias?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ (Coordinates?.GetHashCode() ?? 0);
                return hashCode;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents the coordinates and dimensions of a crop area defined by the image cropper.
    /// </summary>
    public class ImageCropperCropCoordinates : IEquatable<ImageCropperCropCoordinates>
    {
        /// <summary>
        /// Gets or sets the X-coordinate of the top-left corner (X1) of the image crop rectangle.
        /// </summary>
        public decimal X1 { get; set; }

        /// <summary>
        /// Gets or sets the Y1 coordinate (top edge) of the image crop rectangle.
        /// </summary>
        public decimal Y1 { get; set; }

        /// <summary>
        /// Gets or sets the X2 coordinate, representing the right boundary of the image crop rectangle.
        /// </summary>
        public decimal X2 { get; set; }

        /// <summary>
        /// Gets or sets the Y2 coordinate, representing the bottom edge of the image crop rectangle.
        /// </summary>
        public decimal Y2 { get; set; }

        #region IEquatable

        /// <inheritdoc />
        public bool Equals(ImageCropperCropCoordinates? other)
            => ReferenceEquals(this, other) || Equals(this, other);

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || (obj is ImageCropperCropCoordinates other && Equals(this, other));

        private static bool Equals(ImageCropperCropCoordinates? left, ImageCropperCropCoordinates? right)
            => ReferenceEquals(left, right) // deals with both being null, too
               || (!ReferenceEquals(left, null) && !ReferenceEquals(right, null)
                                                && left.X1 == right.X1
                                                && left.X2 == right.X2
                                                && left.Y1 == right.Y1
                                                && left.Y2 == right.Y2);

        public static bool operator ==(ImageCropperCropCoordinates? left, ImageCropperCropCoordinates? right)
            => Equals(left, right);

        public static bool operator !=(ImageCropperCropCoordinates? left, ImageCropperCropCoordinates? right)
            => !Equals(left, right);

        /// <summary>
        /// Returns a hash code for this instance based on the crop coordinates.
        /// </summary>
        /// <returns>A hash code representing the current crop coordinates.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // properties are, practically, readonly
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = X1.GetHashCode();
                hashCode = (hashCode * 397) ^ Y1.GetHashCode();
                hashCode = (hashCode * 397) ^ X2.GetHashCode();
                hashCode = (hashCode * 397) ^ Y2.GetHashCode();
                return hashCode;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        #endregion
    }

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(ImageCropperValue? other)
        => ReferenceEquals(this, other) || Equals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj) || (obj is ImageCropperValue other && Equals(this, other));

    private static bool Equals(ImageCropperValue? left, ImageCropperValue? right)
        => ReferenceEquals(left, right) // deals with both being null, too
           || (!ReferenceEquals(left, null) && !ReferenceEquals(right, null)
                                            && string.Equals(left.Src, right.Src)
                                            && Equals(left.FocalPoint, right.FocalPoint)
                                            && left.ComparableCrops.SequenceEqual(right.ComparableCrops));

    private IEnumerable<ImageCropperCrop> ComparableCrops
        => Crops?.OrderBy(x => x.Alias) ?? Enumerable.Empty<ImageCropperCrop>();

    public static bool operator ==(ImageCropperValue? left, ImageCropperValue? right)
        => Equals(left, right);

    public static bool operator !=(ImageCropperValue? left, ImageCropperValue? right)
        => !Equals(left, right);

    /// <summary>
    /// Returns a hash code for this <see cref="ImageCropperValue"/> instance, based on its property values.
    /// </summary>
    /// <returns>A hash code representing the current <see cref="ImageCropperValue"/> object.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            // properties are, practically, readonly
            // ReSharper disable NonReadonlyMemberInGetHashCode
            var hashCode = Src?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ (FocalPoint?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (Crops?.GetHashCode() ?? 0);
            return hashCode;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
    }

    #endregion
}
