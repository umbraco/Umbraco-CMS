using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be saved
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class ContentItemSave : ContentBaseItemSave<IContent>
    {
        /// <summary>
        /// The language Id for the content variation being saved
        /// </summary>
        [DataMember(Name = "languageId")]
        public int? LanguageId { get; set; } //TODO: Change this to ContentVariationPublish, but this will all change anyways when we can edit all variants at once

        /// <summary>
        /// The template alias to save
        /// </summary>
        [DataMember(Name = "templateAlias")]
        public string TemplateAlias { get; set; }

        [DataMember(Name = "releaseDate")]
        public DateTime? ReleaseDate { get; set; }

        [DataMember(Name = "expireDate")]
        public DateTime? ExpireDate { get; set; }

        /// <summary>
        /// Indicates that these variations should also be published 
        /// </summary>
        [DataMember(Name = "publishVariations")]
        public IEnumerable<ContentVariationPublish> PublishVariations { get; set; }
    }
}
