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
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;
using Umbraco.Web.PropertyEditors.ValueConverters;

namespace Umbraco.Web.Models
{
    [JsonConverter(typeof(NoTypeConverterJsonConverter<MediaCropDataSet>))]
    [TypeConverter(typeof(MediaCropDataSetConverter))]
    [DataContract(Name="mediaCropDataSet")]
    public class MediaCropDataSet : ImageCropDataSet, IHtmlString, IEquatable<MediaCropDataSet>
    {
        [DataMember(Name = "udi")]
        public Udi Udi { get; set; }

        public IPublishedContent MediaItem { get; set; }

        public override string GetCropUrl(string alias, bool useCropDimensions = true, bool useFocalPoint = false, string cacheBusterValue = null)
        {
            return Src + base.GetCropUrl(alias, useCropDimensions, useFocalPoint, cacheBusterValue);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(MediaCropDataSet other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Udi, other.Udi) && Equals(FocalPoint, other.FocalPoint) 
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
            return Equals((MediaCropDataSet) obj);
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
                var hashCode = (Udi != null ? Udi.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (FocalPoint != null ? FocalPoint.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Crops != null ? Crops.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator == (MediaCropDataSet left, MediaCropDataSet right)
        {
            return Equals(left, right);
        }

        public static bool operator != (MediaCropDataSet left, MediaCropDataSet right)
        {
            return !Equals(left, right);
        }
    }
}
