using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be displayed in the back office
    /// </summary>    
    public class ContentItemDisplay : TabbedContentItem<ContentPropertyDisplay, IContent>
    {
        
        [DataMember(Name = "publishDate")]
        public DateTime? PublishDate { get; set; }

    }
}