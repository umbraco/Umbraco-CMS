using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be saved
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public abstract class ContentBaseSave<TPersisted> : ContentItemBasic<ContentPropertyBasic, TPersisted>, IContentSave<TPersisted>
        where TPersisted : IContentBase
    {
        protected ContentBaseSave()
        {
            UploadedFiles = new List<ContentPropertyFile>();
        }

        #region IContentSave
        /// <inheritdoc />
        [DataMember(Name = "action", IsRequired = true)]
        [Required]
        public ContentSaveAction Action { get; set; }

        [IgnoreDataMember]
        public List<ContentPropertyFile> UploadedFiles { get; }

        //These need explicit implementation because we are using internal models
        /// <inheritdoc />
        [IgnoreDataMember]
        TPersisted IContentSave<TPersisted>.PersistedContent { get; set; }

        //These need explicit implementation because we are using internal models
        /// <inheritdoc />
        [IgnoreDataMember]
        ContentItemDto<TPersisted> IContentSave<TPersisted>.ContentDto { get; set; }

        //Non explicit internal getter so we don't need to explicitly cast in our own code
        [IgnoreDataMember]
        internal TPersisted PersistedContent
        {
            get => ((IContentSave<TPersisted>)this).PersistedContent;
            set => ((IContentSave<TPersisted>) this).PersistedContent = value;
        }

        //Non explicit internal getter so we don't need to explicitly cast in our own code
        [IgnoreDataMember]
        internal ContentItemDto<TPersisted> ContentDto
        {
            get => ((IContentSave<TPersisted>)this).ContentDto;
            set => ((IContentSave<TPersisted>) this).ContentDto = value;
        }

        #endregion

    }
}
