using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.PropertyEditors.ValueConverters // fixme MOVE TO MODELS OR SOMETHING
{
    /// <summary>
    /// Represents a value of the image cropper value editor.
    /// </summary>
    [JsonConverter(typeof(NoTypeConverterJsonConverter<ImageCropperValue>))]
    [TypeConverter(typeof(ImageCropperValueTypeConverter))]
    [DataContract(Name="imageCropDataSet")]
    public class ImageCropperValue : IHtmlString, IEquatable<ImageCropperValue>
    {
        /// <summary>
        /// Gets or sets the value source image.
        /// </summary>
        [DataMember(Name="src")]
        public string Src { get; set;}

        /// <summary>
        /// Gets or sets the value focal point.
        /// </summary>
        [DataMember(Name = "focalPoint")]
        public ImageCropperFocalPoint FocalPoint { get; set; }

        /// <summary>
        /// Gets or sets the value crops.
        /// </summary>
        [DataMember(Name = "crops")]
        public IEnumerable<ImageCropperCrop> Crops { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Crops != null ? (Crops.Any() ? JsonConvert.SerializeObject(this) : Src) : string.Empty;
        }

        /// <inheritdoc />
        public string ToHtmlString() => Src;

        /// <summary>
        /// Gets a crop.
        /// </summary>
        public ImageCropperCrop GetCrop(string alias)
        {
            if (Crops == null)
                return null;

            return string.IsNullOrWhiteSpace(alias)
                ? Crops.FirstOrDefault()
                : Crops.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        }

        // fixme was defined in web project, extension methods? why internal?
        internal void AppendCropBaseUrl(StringBuilder url, ImageCropperCrop crop, bool defaultCrop, bool preferFocalPoint)
        {
            if (preferFocalPoint && HasFocalPoint()
                || crop != null && crop.Coordinates == null && HasFocalPoint()
                || defaultCrop && HasFocalPoint())
            {
                url.Append("?center=");
                url.Append(FocalPoint.Top.ToString(CultureInfo.InvariantCulture));
                url.Append(",");
                url.Append(FocalPoint.Left.ToString(CultureInfo.InvariantCulture));
                url.Append("&mode=crop");
            }
            else if (crop != null && crop.Coordinates != null && preferFocalPoint == false)
            {
                url.Append("?crop=");
                url.Append(crop.Coordinates.X1.ToString(CultureInfo.InvariantCulture)).Append(",");
                url.Append(crop.Coordinates.Y1.ToString(CultureInfo.InvariantCulture)).Append(",");
                url.Append(crop.Coordinates.X2.ToString(CultureInfo.InvariantCulture)).Append(",");
                url.Append(crop.Coordinates.Y2.ToString(CultureInfo.InvariantCulture));
                url.Append("&cropmode=percentage");
            }
            else
            {
                url.Append("?anchor=center");
                url.Append("&mode=crop");
            }
        }

        /// <summary>
        /// Gets the value image url for a specified crop.
        /// </summary>
        public string GetCropUrl(string alias, bool useCropDimensions = true, bool useFocalPoint = false, string cacheBusterValue = null)
        {
            var crop = GetCrop(alias);

            // could not find a crop with the specified, non-empty, alias
            if (crop == null && !string.IsNullOrWhiteSpace(alias))
                return null;

            var url = new StringBuilder();

            AppendCropBaseUrl(url, crop, string.IsNullOrWhiteSpace(alias), useFocalPoint);

            if (crop != null && useCropDimensions)
            {
                url.Append("&width=").Append(crop.Width);
                url.Append("&height=").Append(crop.Height);
            }

            if (cacheBusterValue != null)
                url.Append("&rnd=").Append(cacheBusterValue);

            return url.ToString();
        }

        /// <summary>
        /// Gets the value image url for a specific width and height.
        /// </summary>
        public string GetCropUrl(int width, int height, bool useFocalPoint = false, string cacheBusterValue = null)
        {
            var url = new StringBuilder();

            AppendCropBaseUrl(url, null, true, useFocalPoint);

            url.Append("&width=").Append(width);
            url.Append("&height=").Append(height);

            if (cacheBusterValue != null)
                url.Append("&rnd=").Append(cacheBusterValue);

            return url.ToString();
        }

        /// <summary>
        /// Determines whether the value has a focal point.
        /// </summary>
        /// <returns></returns>
        public bool HasFocalPoint()
            => FocalPoint != null && (FocalPoint.Left != 0.5m || FocalPoint.Top != 0.5m);

        /// <summary>
        /// Determines whether the value has a specified crop.
        /// </summary>
        public bool HasCrop(string alias)
            => Crops.Any(x => x.Alias == alias);

        /// <summary>
        /// Determines whether the value has a source image.
        /// </summary>
        public bool HasImage()
            => !string.IsNullOrWhiteSpace(Src);

        /// <summary>
        /// Applies a configuration.
        /// </summary>
        /// <remarks>Ensures that all crops defined in the configuration exists in the value.</remarks>
        internal void ApplyConfiguration(ImageCropperConfiguration configuration)
        {
            // merge the crop values - the alias + width + height comes from
            // configuration, but each crop can store its own coordinates

            if (Crops == null) return;

            var configuredCrops = configuration?.Crops;
            if (configuredCrops == null) return;

            var crops = Crops.ToList();

            foreach (var configuredCrop in configuredCrops)
            {
                var crop = crops.FirstOrDefault(x => x.Alias == configuredCrop.Alias);
                if (crop != null)
                {
                    // found, apply the height & width
                    crop.Width = configuredCrop.Width;
                    crop.Height = configuredCrop.Height;
                }
                else
                {
                    // not found, add
                    crops.Add(new ImageCropperCrop
                    {
                        Alias = configuredCrop.Alias,
                        Width = configuredCrop.Width,
                        Height = configuredCrop.Height
                    });
                }
            }

            // assume we don't have to remove the crops in value, that
            // are not part of configuration anymore?

            Crops = crops;
        }

        #region IEquatable

        /// <inheritdoc />
        public bool Equals(ImageCropperValue other)
            => ReferenceEquals(this, other) || Equals(this, other);

        /// <inheritdoc />
        public override bool Equals(object obj)
            => ReferenceEquals(this, obj) || obj is ImageCropperValue other && Equals(this, other);

        private static bool Equals(ImageCropperValue left, ImageCropperValue right)
            => ReferenceEquals(left, right) // deals with both being null, too
                || !ReferenceEquals(left, null) && !ReferenceEquals(right, null)
                   && string.Equals(left.Src, right.Src)
                   && Equals(left.FocalPoint, right.FocalPoint)
                   && left.ComparableCrops.SequenceEqual(right.ComparableCrops);

        private IEnumerable<ImageCropperCrop> ComparableCrops
            => Crops?.OrderBy(x => x.Alias) ?? Enumerable.Empty<ImageCropperCrop>();

        public static bool operator ==(ImageCropperValue left, ImageCropperValue right)
            => Equals(left, right);

        public static bool operator !=(ImageCropperValue left, ImageCropperValue right)
            => !Equals(left, right);

        public override int GetHashCode()
        {
            unchecked
            {
                // properties are, practically, readonly
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = Src?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (FocalPoint?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Crops?.GetHashCode() ?? 0);
                return hashCode;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        #endregion

        [DataContract(Name = "imageCropFocalPoint")]
        public class ImageCropperFocalPoint : IEquatable<ImageCropperFocalPoint>
        {
            [DataMember(Name = "left")]
            public decimal Left { get; set; }

            [DataMember(Name = "top")]
            public decimal Top { get; set; }

            #region IEquatable

            /// <inheritdoc />
            public bool Equals(ImageCropperFocalPoint other)
                => ReferenceEquals(this, other) || Equals(this, other);

            /// <inheritdoc />
            public override bool Equals(object obj)
                => ReferenceEquals(this, obj) || obj is ImageCropperFocalPoint other && Equals(this, other);

            private static bool Equals(ImageCropperFocalPoint left, ImageCropperFocalPoint right)
                => ReferenceEquals(left, right) // deals with both being null, too
                   || !ReferenceEquals(left, null) && !ReferenceEquals(right, null)
                       && left.Left == right.Left
                       && left.Top == right.Top;

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
                    return (Left.GetHashCode()*397) ^ Top.GetHashCode();
                    // ReSharper restore NonReadonlyMemberInGetHashCode
                }
            }

            #endregion
        }

        [DataContract(Name = "imageCropData")]
        public class ImageCropperCrop : IEquatable<ImageCropperCrop>
        {
            [DataMember(Name = "alias")]
            public string Alias { get; set; }

            [DataMember(Name = "width")]
            public int Width { get; set; }

            [DataMember(Name = "height")]
            public int Height { get; set; }

            [DataMember(Name = "coordinates")]
            public ImageCropperCropCoordinates Coordinates { get; set; }

            #region IEquatable

            /// <inheritdoc />
            public bool Equals(ImageCropperCrop other)
                => ReferenceEquals(this, other) || Equals(this, other);

            /// <inheritdoc />
            public override bool Equals(object obj)
                => ReferenceEquals(this, obj) || obj is ImageCropperCrop other && Equals(this, other);

            private static bool Equals(ImageCropperCrop left, ImageCropperCrop right)
                => ReferenceEquals(left, right) // deals with both being null, too
                    || !ReferenceEquals(left, null) && !ReferenceEquals(right, null)
                       && string.Equals(left.Alias, right.Alias)
                       && left.Width == right.Width
                       && left.Height == right.Height
                       && Equals(left.Coordinates, right.Coordinates);

            public static bool operator ==(ImageCropperCrop left, ImageCropperCrop right)
                => Equals(left, right);

            public static bool operator !=(ImageCropperCrop left, ImageCropperCrop right)
                => !Equals(left, right);

            public override int GetHashCode()
            {
                unchecked
                {
                    // properties are, practically, readonly
                    // ReSharper disable NonReadonlyMemberInGetHashCode
                    var hashCode = Alias?.GetHashCode() ?? 0;
                    hashCode = (hashCode*397) ^ Width;
                    hashCode = (hashCode*397) ^ Height;
                    hashCode = (hashCode*397) ^ (Coordinates?.GetHashCode() ?? 0);
                    return hashCode;
                    // ReSharper restore NonReadonlyMemberInGetHashCode
                }
            }

            #endregion
        }

        [DataContract(Name = "imageCropCoordinates")]
        public class ImageCropperCropCoordinates : IEquatable<ImageCropperCropCoordinates>
        {
            [DataMember(Name = "x1")]
            public decimal X1 { get; set; }

            [DataMember(Name = "y1")]
            public decimal Y1 { get; set; }

            [DataMember(Name = "x2")]
            public decimal X2 { get; set; }

            [DataMember(Name = "y2")]
            public decimal Y2 { get; set; }

            #region IEquatable
            
            /// <inheritdoc />
            public bool Equals(ImageCropperCropCoordinates other)
                => ReferenceEquals(this, other) || Equals(this, other);

            /// <inheritdoc />
            public override bool Equals(object obj)
                => ReferenceEquals(this, obj) || obj is ImageCropperCropCoordinates other && Equals(this, other);

            private static bool Equals(ImageCropperCropCoordinates left, ImageCropperCropCoordinates right)
                => ReferenceEquals(left, right) // deals with both being null, too
                   || !ReferenceEquals(left, null) && !ReferenceEquals(right, null)
                      && left.X1 == right.X1
                      && left.X2 == right.X2
                      && left.Y1 == right.Y1
                      && left.Y2 == right.Y2;

            public static bool operator ==(ImageCropperCropCoordinates left, ImageCropperCropCoordinates right)
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
                    hashCode = (hashCode*397) ^ Y1.GetHashCode();
                    hashCode = (hashCode*397) ^ X2.GetHashCode();
                    hashCode = (hashCode*397) ^ Y2.GetHashCode();
                    return hashCode;
                    // ReSharper restore NonReadonlyMemberInGetHashCode
                }
            }

            #endregion
        }
    }
}
