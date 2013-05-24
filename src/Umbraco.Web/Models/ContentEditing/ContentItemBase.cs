using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a basic content item
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public abstract class ContentItemBase<T>
        where T: ContentPropertyBase
    {
        protected ContentItemBase()
        {
            //ensure its not null
            Properties = new List<T>();
        }

        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }

        [DataMember(Name = "properties")]
        public IEnumerable<T> Properties { get; set; }

        protected bool Equals(ContentItemBase<T> other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as ContentItemBase<T>;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}