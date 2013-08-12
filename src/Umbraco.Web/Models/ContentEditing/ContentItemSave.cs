using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be saved
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class ContentItemSave<TPersisted> : ContentItemBasic<ContentPropertyBasic, TPersisted>, IHaveUploadedFiles 
        where TPersisted : IContentBase
    {
        public ContentItemSave()
        {
            UploadedFiles = new List<ContentItemFile>();
        }

        /// <summary>
        /// The action to perform when saving this content item
        /// </summary>
        [DataMember(Name = "action", IsRequired = true)]
        [Required]
        public ContentSaveAction Action { get; set; }

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
        /// The collection of files uploaded
        /// </summary>
        [JsonIgnore]
        public List<ContentItemFile> UploadedFiles { get; private set; }
    }
}