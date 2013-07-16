using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a basic content item
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class ContentItemBasic<T, TPersisted>
        where T: ContentPropertyBasic
        where TPersisted : IContentBase
    {
        public ContentItemBasic()
        {
            //ensure its not null
            _properties = new List<T>();
        }

        private IEnumerable<T> _properties;

        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }

        [DataMember(Name = "name", IsRequired = true)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Name { get; set; }

        [DataMember(Name = "properties")]
        public virtual IEnumerable<T> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        [DataMember(Name = "updateDate")]
        public DateTime UpdateDate { get; set; }

        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "parentId", IsRequired = true)]
        [Required]
        public int ParentId { get; set; }

        [DataMember(Name = "owner")]
        public UserBasic Owner { get; set; }
        
        [DataMember(Name = "updator")]
        public UserBasic Updator { get; set; }

        [DataMember(Name = "contentTypeAlias", IsRequired = true)]
        [Required(AllowEmptyStrings = false)]
        public string ContentTypeAlias { get; set; }

        /// <summary>
        /// The real persisted content object
        /// </summary>
        [JsonIgnore]
        internal TPersisted PersistedContent { get; set; }

        /// <summary>
        /// The DTO object used to gather all required content data including data type information etc... for use with validation
        /// </summary>
        /// <remarks>
        /// We basically use this object to hydrate all required data from the database into one object so we can validate everything we need
        /// instead of having to look up all the data individually.
        /// </remarks>
        [JsonIgnore]
        internal ContentItemDto<TPersisted> ContentDto { get; set; }

        protected bool Equals(ContentItemBasic<T, TPersisted> other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as ContentItemBasic<T, TPersisted>;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}