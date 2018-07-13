using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents the variant info for a content item
    /// </summary>
    [DataContract(Name = "contentVariant", Namespace = "")]
    public class ContentVariantDisplay : ITabbedContentItem<ContentPropertyDisplay>
    {
        public ContentVariantDisplay()
        {
            Tabs = new List<Tab<ContentPropertyDisplay>>();
        }

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// Defines the tabs containing display properties
        /// </summary>
        [DataMember(Name = "tabs")]
        public IEnumerable<Tab<ContentPropertyDisplay>> Tabs { get; set; }

        // note
        // once a [DataContract] has been defined on a class, with a [DataMember] property,
        // one simply cannot ignore that property anymore - [IgnoreDataMember] on an overriden
        // property is ignored, and 'newing' the property means that it's the base property
        // which is used
        //
        // OTOH, Json.NET is happy having [JsonIgnore] on overrides, even though the base
        // property is [JsonProperty]. so, forcing [JsonIgnore] here, but really, we should
        // rething the whole thing.

        /// <summary>
        /// Override the properties property to ensure we don't serialize this
        /// and to simply return the properties based on the properties in the tabs collection
        /// </summary>
        /// <remarks>
        /// This property cannot be set
        /// </remarks>
        [IgnoreDataMember]
        [JsonIgnore] // see note above on IgnoreDataMember vs JsonIgnore
        public IEnumerable<ContentPropertyDisplay> Properties
        {
            get => Tabs.SelectMany(x => x.Properties);
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The language/culture assigned to this content variation
        /// </summary>
        /// <remarks>
        /// If this is null it means this content variant is an invariant culture
        /// </remarks>
        [DataMember(Name = "language")]
        public Language Language { get; set; }

        [DataMember(Name = "segment")]
        public string Segment { get; set; }

        [DataMember(Name = "state")]
        public string PublishedState { get; set; }

        [DataMember(Name = "updateDate")]
        public DateTime UpdateDate { get; set; }

        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        //[DataMember(Name = "published")]
        //public bool Published { get; set; }

        [DataMember(Name = "publishDate")]
        public DateTime? PublishDate { get; set; }

        /// <summary>
        /// Determines if the content variant for this culture has been created
        /// </summary>
        [DataMember(Name = "exists")]
        public bool Exists { get; set; }

        [DataMember(Name = "isEdited")]
        public bool IsEdited { get; set; }

        ///// <summary>
        ///// Determines if this is the variant currently being edited
        ///// </summary>
        //[DataMember(Name = "current")]
        //public bool IsCurrent { get; set; }

        ///// <summary>
        ///// If the variant is a required variant for validation purposes
        ///// </summary>
        //[DataMember(Name = "mandatory")]
        //public bool Mandatory { get; set; }
    }
}
