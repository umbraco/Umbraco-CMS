using System;
using System.Runtime.Serialization;
using Umbraco.Core.Dynamics;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "imageCropFocalPoint")]
    public class ImageCropFocalPoint : CaseInsensitiveDynamicObject<ImageCropFocalPoint>, IEquatable<ImageCropFocalPoint>
    {
        [DataMember(Name = "left")]
        public decimal Left { get; set; }

        [DataMember(Name = "top")]
        public decimal Top { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ImageCropFocalPoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Left == other.Left && Top == other.Top;
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
            return Equals((ImageCropFocalPoint) obj);
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
                return (Left.GetHashCode()*397) ^ Top.GetHashCode();
            }
        }

        public static bool operator ==(ImageCropFocalPoint left, ImageCropFocalPoint right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImageCropFocalPoint left, ImageCropFocalPoint right)
        {
            return !Equals(left, right);
        }
    }
}