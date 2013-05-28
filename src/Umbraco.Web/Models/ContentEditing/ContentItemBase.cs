using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.Models;

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

        /// <summary>
        /// The real persisted content object
        /// </summary>
        [JsonIgnore]
        public IContent PersistedContent { get; set; }

        /// <summary>
        /// The DTO object used to gather all required content data including data type information etc... for use with validation
        /// </summary>
        /// <remarks>
        /// We basically use this object to hydrate all required data from the database into one object so we can validate everything we need
        /// instead of having to look up all the data individually.
        /// </remarks>
        [JsonIgnore]
        internal ContentItemDto ContentDto { get; set; }

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