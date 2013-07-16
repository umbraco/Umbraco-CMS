using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be displayed in the back office
    /// </summary>    
    [DataContract(Name = "content", Namespace = "")]
    public class ContentItemDisplay : TabbedContentItem<ContentPropertyDisplay, IContent>
    {
        
        [DataMember(Name = "publishDate")]
        public DateTime? PublishDate { get; set; }

        /// <summary>
        /// This is used for validation of a content item.
        /// </summary>
        /// <remarks>
        /// A content item can be invalid but still be saved. This occurs when there's property validation errors, we will
        /// still save the item but it cannot be published. So we need a way of returning validation errors as well as the
        /// updated model.
        /// </remarks>
        [DataMember(Name = "ModelState")]
        public IDictionary<string, object> Errors { get; set; }

    }
}