// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Represents a value of the image cropper value editor.
/// </summary>
[JsonConverter(typeof(NoTypeConverterJsonConverter<ImageCropperValue>))]
[TypeConverter(typeof(ImageCropperValueTypeConverter))]
[DataContract(Name = "imageCropDataSet")]
public class ImageCropperValue : IHtmlEncodedString, IEquatable<ImageCropperValue>
{
    /// <summary>
    ///     Gets or sets the value source image.
    /// </summary>
    [DataMember(Name = "src")]
    public string? Src { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the value focal point.
    /// </summary>
    [DataMember(Name = "focalPoint")]
    public ImageCropperFocalPoint? FocalPoint { get; set; }

    /// <summary>
    ///     Gets or sets the value crops.
    /// </summary>
    [DataMember(Name = "crops")]
    public IEnumerable<ImageCropperCrop>? Crops { get; set; }

    /// <inheritdoc />
    public string? ToHtmlString() => Src;

    /// <inheritdoc />
    public override string? ToString()
        => HasCrops() || HasFocalPoint() ? JsonConvert.SerializeObject(this, Formatting.None) : Src;

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
                Crop = new ImageUrlGenerationOptions.CropCoordinates(crop.Coordinates.X1, crop.Coordinates.Y1,
                    crop.Coordinates.X2, crop.Coordinates.Y2)
            };
        }

        return new ImageUrlGenerationOptions(url);
    }

    /// <summary>
    ///     Gets the value image URL for a specified crop.
    /// </summary>
    public string? GetCropUrl(string alias, IImageUrlGenerator imageUrlGenerator, bool useCropDimensions = true,
        bool useFocalPoint = false, string? cacheBusterValue = null)
    {
        ImageCropperCrop? crop = GetCrop(alias);

        // could not find a crop with the specified, non-empty, alias
        if (crop is null && !string.IsNullOrWhiteSpace(alias))
        {
            return null;
        }

        ImageUrlGenerationOptions options =
            GetCropBaseOptions(null, crop, useFocalPoint || string.IsNullOrWhiteSpace(alias));

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
    public string? GetCropUrl(int width, int height, IImageUrlGenerator imageUrlGenerator,
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
    public bool HasCrops()
        => Crops is not null && Crops.Any();

    /// <summary>
    ///     Determines whether the value has a specified crop.
    /// </summary>
    public bool HasCrop(string alias)
        => Crops is not null && Crops.Any(x => x.Alias == alias);

    /// <summary>
    ///     Determines whether the value has a source image.
    /// </summary>
    public bool HasImage()
        => !string.IsNullOrWhiteSpace(Src);

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
    /// <param name="value">The image cropper value.</param>
    public static void Prune(JObject? value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.TryGetValue("crops", out JToken? crops))
        {
            if (crops.HasValues)
            {
                foreach (JObject crop in crops.Values<JObject>().WhereNotNull().ToList())
                {
                    if (crop.TryGetValue("coordinates", out JToken? coordinates) == false ||
                        coordinates.HasValues == false)
                    {
                        // Remove crop without coordinates
                        crop.Remove();
                        continue;
                    }

                    // Width/height are already stored in the crop configuration
                    crop.Remove("width");
                    crop.Remove("height");
                }
            }

            if (crops.HasValues == false)
            {
                // Remove empty crops
                value.Remove("crops");
            }
        }

        if (value.TryGetValue("focalPoint", out JToken? focalPoint) &&
            (focalPoint.HasValues == false ||
             (focalPoint.Value<decimal>("top") == 0.5m && focalPoint.Value<decimal>("left") == 0.5m)))
        {
            // Remove empty/default focal point
            value.Remove("focalPoint");
        }
    }

    [DataContract(Name = "imageCropFocalPoint")]
    public class ImageCropperFocalPoint : IEquatable<ImageCropperFocalPoint>
    {
        [DataMember(Name = "left")] public decimal Left { get; set; }

        [DataMember(Name = "top")] public decimal Top { get; set; }

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

    [DataContract(Name = "imageCropData")]
    public class ImageCropperCrop : IEquatable<ImageCropperCrop>
    {
        [DataMember(Name = "alias")] public string? Alias { get; set; }

        [DataMember(Name = "width")] public int Width { get; set; }

        [DataMember(Name = "height")] public int Height { get; set; }

        [DataMember(Name = "coordinates")] public ImageCropperCropCoordinates? Coordinates { get; set; }

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

    [DataContract(Name = "imageCropCoordinates")]
    public class ImageCropperCropCoordinates : IEquatable<ImageCropperCropCoordinates>
    {
        [DataMember(Name = "x1")] public decimal X1 { get; set; }

        [DataMember(Name = "y1")] public decimal Y1 { get; set; }

        [DataMember(Name = "x2")] public decimal X2 { get; set; }

        [DataMember(Name = "y2")] public decimal Y2 { get; set; }

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

        public static bool operator !=(ImageCropperCropCoordinates left, ImageCropperCropCoordinates right)
            => !Equals(left, right);

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
