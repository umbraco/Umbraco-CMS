using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Serialization;
using Umbraco.Web.PropertyEditors.ValueConverters;

namespace Umbraco.Web.Models
{
    [JsonConverter(typeof(NoTypeConverterJsonConverter<ImageCropDataSet>))]
    [TypeConverter(typeof(ImageCropDataSetConverter))]
    [DataContract(Name="imageCropDataSet")]
    public class ImageCropDataSet : CaseInsensitiveDynamicObject<ImageCropDataSet>, IHtmlString, IEquatable<ImageCropDataSet>
    {   

        [DataMember(Name="src")]
        public string Src { get; set;}

        [DataMember(Name = "focalPoint")]
        public ImageCropFocalPoint FocalPoint { get; set; }

        [DataMember(Name = "crops")]
        public IEnumerable<ImageCropData> Crops { get; set; }

        public string GetCropUrl(string alias, bool useCropDimensions = true, bool useFocalPoint = false, string cacheBusterValue = null)
        {

            var crop = Crops.GetCrop(alias);

            if (crop == null && string.IsNullOrEmpty(alias) == false)
            {
                return null;
            }

            var sb = new StringBuilder();

            var cropBaseUrl = this.GetCropBaseUrl(alias, useFocalPoint);
            if (cropBaseUrl != null)
            {
                sb.Append(cropBaseUrl);
            }

            if (crop != null && useCropDimensions)
            {
                sb.Append("&width=").Append(crop.Width);
                sb.Append("&height=").Append(crop.Height);
            }

            if (cacheBusterValue != null)
            {
                sb.Append("&rnd=").Append(cacheBusterValue);
            }

            return sb.ToString();

        }

        public bool HasFocalPoint()
        {
            return FocalPoint != null && FocalPoint.Left != 0.5m && FocalPoint.Top != 0.5m;
        }

        public bool HasCrop(string alias)
        {
            return Crops.Any(x => x.Alias == alias);
        }

        public bool HasImage()
        {
            return string.IsNullOrEmpty(Src);
        }

        public string ToHtmlString()
        {
            return this.Src;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// If there are crops defined, it will return the JSON value, otherwise it will just return the Src value
        /// </returns>
        public override string ToString()
        {
            return Crops.Any() ? JsonConvert.SerializeObject(this) : Src;
        }
               

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ImageCropDataSet other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Src, other.Src) && Equals(FocalPoint, other.FocalPoint) 
                && Crops.SequenceEqual(other.Crops);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ImageCropDataSet) obj);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Src != null ? Src.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (FocalPoint != null ? FocalPoint.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Crops != null ? Crops.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ImageCropDataSet left, ImageCropDataSet right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImageCropDataSet left, ImageCropDataSet right)
        {
            return !Equals(left, right);
        }
    }
}