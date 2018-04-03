using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents the variant info for a content item
    /// </summary>
    [DataContract(Name = "contentVariant", Namespace = "")]
    public class ContentVariation
    {
        /// <summary>
        /// The content name of the variant
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }
        
        [DataMember(Name = "language")]
        public Language Language { get; set; }

        [DataMember(Name = "segment")]
        public string Segment { get; set; }

        //fixme not sure if we need these dates as metadata for displaying the variant info in the drop down?
        // when we move to being able to edit all variants and switching then this might be irrelevant

        [DataMember(Name = "publishDate")]
        public DateTime? PublishDate { get; set; }

        [DataMember(Name = "releaseDate")]
        public DateTime? ReleaseDate { get; set; }

        [DataMember(Name = "removeDate")]
        public DateTime? ExpireDate { get; set; }

        [DataMember(Name = "state")]
        public string PublishedState { get; set; }

        /// <summary>
        /// Determines if the content variant for this culture has been created
        /// </summary>
        [DataMember(Name = "exists")]
        public bool Exists { get; set; }

        /// <summary>
        /// Determines if this is the variant currently being edited
        /// </summary>
        [DataMember(Name = "current")]
        public bool IsCurrent { get; set; }
    }
}
