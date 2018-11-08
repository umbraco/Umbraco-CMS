using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        /// <summary>
        /// Boolean indicating if this item is published or not based on it's <see cref="State"/>
        /// </summary>
        [DataMember(Name = "published")]
        public bool Published => State == ContentSavedState.Published || State == ContentSavedState.PublishedPendingChanges;

        /// <summary>
        /// Determines if the content item is a draft
        /// </summary>
        [DataMember(Name = "edited")]
        public bool Edited { get; set; }

        [DataMember(Name = "owner")]
        public UserProfile Owner { get; set; }

        [DataMember(Name = "updater")]
        public UserProfile Updater { get; set; }

        [DataMember(Name = "contentTypeAlias", IsRequired = true)]
        [Required(AllowEmptyStrings = false)]
        public string ContentTypeAlias { get; set; }

        [DataMember(Name = "sortOrder")]
        public int SortOrder { get; set; }

        /// <summary>
        /// The saved/published state of an item
        /// </summary>
        /// <remarks>
        /// This is nullable since it's only relevant for content (non-content like media + members will be null)
        /// </remarks>
        [DataMember(Name = "state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentSavedState? State { get; set; }

        [DataMember(Name = "variesByCulture")]
        public bool VariesByCulture { get; set; }

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
    public class ContentItemBasic<T> : ContentItemBasic, IContentProperties<T>
        where T : ContentPropertyBasic
    {
        public ContentItemBasic()
        {
            //ensure its not null
            _properties = Enumerable.Empty<T>();
        }

        private IEnumerable<T> _properties;

        [DataMember(Name = "properties")]
        public virtual IEnumerable<T> Properties
        {
            get => _properties;
            set => _properties = value;
        }

    }
}
