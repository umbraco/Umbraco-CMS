using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Validation;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a basic content item
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class ContentItemBasic : EntityBasic
    {   

        [DataMember(Name = "updateDate")]
        public DateTime UpdateDate { get; set; }

        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "published")]
        public bool Published { get; set; }

        [DataMember(Name = "hasPublishedVersion")]
        public bool HasPublishedVersion { get; set; }

        [DataMember(Name = "owner")]
        public UserProfile Owner { get; set; }

        [DataMember(Name = "updater")]
        public UserProfile Updater { get; set; }

        [DataMember(Name = "contentTypeAlias", IsRequired = true)]
        [Required(AllowEmptyStrings = false)]
        public string ContentTypeAlias { get; set; }

        [DataMember(Name = "sortOrder")]
        public int SortOrder { get; set; }
        
        protected bool Equals(ContentItemBasic other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as ContentItemBasic;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    /// <summary>
    /// A model representing a basic content item with properties
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class ContentItemBasic<T, TPersisted> : ContentItemBasic
        where T : ContentPropertyBasic
        where TPersisted : IContentBase
    {
        public ContentItemBasic()
        {
            //ensure its not null
            _properties = new List<T>();
        }

        private IEnumerable<T> _properties;

        [DataMember(Name = "properties")]
        public virtual IEnumerable<T> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        /// <summary>
        /// The real persisted content object - used during inbound model binding
        /// </summary>
        /// <remarks>
        /// This is not used for outgoing model information.
        /// </remarks>
        [IgnoreDataMember]
        internal TPersisted PersistedContent { get; set; }

        /// <summary>
        /// The DTO object used to gather all required content data including data type information etc... for use with validation - used during inbound model binding
        /// </summary>
        /// <remarks>
        /// We basically use this object to hydrate all required data from the database into one object so we can validate everything we need
        /// instead of having to look up all the data individually.
        /// This is not used for outgoing model information.
        /// </remarks>
        [IgnoreDataMember]
        internal ContentItemDto<TPersisted> ContentDto { get; set; }


    }
}