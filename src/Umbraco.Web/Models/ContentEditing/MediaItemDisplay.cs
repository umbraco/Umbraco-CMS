﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a media item to be displayed in the back office
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class MediaItemDisplay : ListViewAwareContentItemDisplayBase<ContentPropertyDisplay>
    {
        public MediaItemDisplay()
        {
            ContentApps = new List<ContentApp>();
        }

        [DataMember(Name = "contentType")]
        public ContentTypeBasic ContentType { get; set; }

        [DataMember(Name = "mediaLink")]
        public string MediaLink { get; set; }

        [DataMember(Name = "apps")]
        public IEnumerable<ContentApp> ContentApps { get; set; }
    }
}
