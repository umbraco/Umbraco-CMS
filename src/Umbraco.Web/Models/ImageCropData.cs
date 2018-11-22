using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Umbraco.Core.Dynamics;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "imageCropData")]
    public class ImageCropData : CaseInsensitiveDynamicObject<ImageCropData>, IEquatable<ImageCropData>
    {
        [DataMember(Name = "alias")]
        public string Alias { get; set; }
        
        [DataMember(Name = "width")]
        public int Width { get; set; }

        [DataMember(Name = "height")]
        public int Height { get; set; }

        [DataMember(Name = "coordinates")]
        public ImageCropCoordinates Coordinates { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ImageCropData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Alias, other.Alias) && Width == other.Width && Height == other.Height && Equals(Coordinates, other.Coordinates);
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
            return Equals((ImageCropData) obj);
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
                var hashCode = (Alias != null ? Alias.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Width;
                hashCode = (hashCode*397) ^ Height;
                hashCode = (hashCode*397) ^ (Coordinates != null ? Coordinates.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ImageCropData left, ImageCropData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImageCropData left, ImageCropData right)
        {
            return !Equals(left, right);
        }
    }

}
