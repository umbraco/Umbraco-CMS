using System;
using System.Runtime.Serialization;
using Umbraco.Core.Dynamics;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "imageCropCoordinates")]
    public class ImageCropCoordinates : CaseInsensitiveDynamicObject<ImageCropCoordinates>, IEquatable<ImageCropCoordinates>
    {
        [DataMember(Name = "x1")]
        public decimal X1 { get; set; }

        [DataMember(Name = "y1")]
        public decimal Y1 { get; set; }

        [DataMember(Name = "x2")]
        public decimal X2 { get; set; }

        [DataMember(Name = "y2")]
        public decimal Y2 { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ImageCropCoordinates other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X1 == other.X1 && Y1 == other.Y1 && X2 == other.X2 && Y2 == other.Y2;
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
            return Equals((ImageCropCoordinates) obj);
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
                var hashCode = X1.GetHashCode();
                hashCode = (hashCode*397) ^ Y1.GetHashCode();
                hashCode = (hashCode*397) ^ X2.GetHashCode();
                hashCode = (hashCode*397) ^ Y2.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ImageCropCoordinates left, ImageCropCoordinates right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImageCropCoordinates left, ImageCropCoordinates right)
        {
            return !Equals(left, right);
        }
    }
}