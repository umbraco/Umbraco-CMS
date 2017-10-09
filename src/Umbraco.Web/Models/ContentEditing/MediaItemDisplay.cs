using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be displayed in the back office
    /// </summary>    
    [DataContract(Name = "content", Namespace = "")]
    public class MediaItemDisplay : ListViewAwareContentItemDisplayBase<ContentPropertyDisplay, IMedia>
    {
        [DataMember(Name = "contentType")]
        public ContentTypeBasic ContentType { get; set; }

        [DataMember(Name = "mediaLink")]
        public string MediaLink { get; set; }
    }
}