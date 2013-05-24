using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be saved
    /// </summary>
    public class ContentItemSave : ContentItemBase<ContentPropertyBase>
    {
        public ContentItemSave()
        {
            UploadedFiles = new List<ContentItemFile>();
        }

        /// <summary>
        /// The collection of files uploaded
        /// </summary>
        [JsonIgnore]
        public List<ContentItemFile> UploadedFiles { get; private set; }
    }
}