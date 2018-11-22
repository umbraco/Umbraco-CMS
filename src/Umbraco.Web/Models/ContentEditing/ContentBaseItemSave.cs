using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content base item to be saved
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public abstract class ContentBaseItemSave<TPersisted> : ContentItemBasic<ContentPropertyBasic, TPersisted>, IHaveUploadedFiles 
        where TPersisted : IContentBase   
    {
        protected ContentBaseItemSave()
        {
            UploadedFiles = new List<ContentItemFile>();
        }

        /// <summary>
        /// The action to perform when saving this content item
        /// </summary>
        [DataMember(Name = "action", IsRequired = true)]
        [Required]
        public ContentSaveAction Action { get; set; }

        [IgnoreDataMember]
        public List<ContentItemFile> UploadedFiles { get; private set; }
    }
}