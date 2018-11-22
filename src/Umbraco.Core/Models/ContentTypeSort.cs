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

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ContentTypeSort(int id, int sortOrder)
        {
            Id = new Lazy<int>(() => id);
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
                //The hash code will just be the alias if one is assigned, otherwise it will be the hash code of the Id.
                //In some cases the alias can be null of the non lazy ctor is used, in that case, the lazy Id will already have a value created.
                return Alias != null ? Alias.GetHashCode() : (Id.Value.GetHashCode() * 397);
            }
        }

    }
}