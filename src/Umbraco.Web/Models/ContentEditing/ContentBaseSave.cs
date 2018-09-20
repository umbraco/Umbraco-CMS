﻿using System.Collections.Generic;
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
    public abstract class ContentBaseSave<TPersisted> : ContentItemBasic<ContentPropertyBasic>, IContentSave<TPersisted>
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
        
        //Non explicit internal getter so we don't need to explicitly cast in our own code
        [IgnoreDataMember]
        internal TPersisted PersistedContent
        {
            get => ((IContentSave<TPersisted>)this).PersistedContent;
            set => ((IContentSave<TPersisted>) this).PersistedContent = value;
        }

        /// <summary>
        /// The property DTO object is used to gather all required property data including data type information etc... for use with validation - used during inbound model binding
        /// </summary>
        /// <remarks>
        /// We basically use this object to hydrate all required data from the database into one object so we can validate everything we need
        /// instead of having to look up all the data individually.
        /// This is not used for outgoing model information.
        /// </remarks>
        [IgnoreDataMember]
        internal ContentPropertyCollectionDto PropertyCollectionDto { get; set; }

        #endregion

    }
}
