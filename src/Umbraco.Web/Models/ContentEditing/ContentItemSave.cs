using System;
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
        public int? LanguageId { get; set; }

        /// <summary>
        /// The template alias to save
        /// </summary>
        [DataMember(Name = "templateAlias")]
        public string TemplateAlias { get; set; }

        [DataMember(Name = "releaseDate")]
        public DateTime? ReleaseDate { get; set; }

        [DataMember(Name = "expireDate")]
        public DateTime? ExpireDate { get; set; }

    }
}
