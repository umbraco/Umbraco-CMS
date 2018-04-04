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
