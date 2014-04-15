using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a POCO for setting sort order on a ContentType reference
    /// </summary>
    public class ContentTypeSort : IValueObject, IDeepCloneable
    {
        public ContentTypeSort()
        {
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
            return Id.Value.Equals(other.Id.Value) && string.Equals(Alias, other.Alias);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContentTypeSort) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode()*397) ^ Alias.GetHashCode();
            }
        }

    }
}