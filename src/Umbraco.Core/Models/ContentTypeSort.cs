using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a POCO for setting sort order on a ContentType reference
    /// </summary>
    public class ContentTypeSort : IValueObject, IDeepCloneable
    {
        [Obsolete("This parameterless constructor should never be used")]
        public ContentTypeSort()
        {
            
        }

        public ContentTypeSort(Lazy<int> id, int sortOrder)
        {
            Id = id;
            SortOrder = sortOrder;
        }

        public ContentTypeSort(Lazy<int> id, int sortOrder, string @alias)
        {
            Id = id;
            SortOrder = sortOrder;
            Alias = alias;
        }

        /// <summary>
        /// Gets or sets the Id of the ContentType
        /// </summary>
        public Lazy<int> Id { get; set; }

        /// <summary>
        /// Gets or sets the Sort Order of the ContentType
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the Alias of the ContentType
        /// </summary>
        public string Alias { get; set; }


        public object DeepClone()
        {
            var clone = (ContentTypeSort)MemberwiseClone();
            var id = Id.Value;
            clone.Id = new Lazy<int>(() => id);
            return clone;
        }

        protected bool Equals(ContentTypeSort other)
        {
            return Id.Equals(other.Id) && string.Equals(Alias, other.Alias);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
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
            return Equals((ContentTypeSort) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.Value.GetHashCode()*397) ^ (Alias != null ? Alias.GetHashCode() : 0);
            }
        }

    }
}