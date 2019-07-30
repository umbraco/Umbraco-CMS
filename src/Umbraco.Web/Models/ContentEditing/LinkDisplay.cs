﻿using System.Runtime.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "link", Namespace = "")]
    internal class LinkDisplay
    {
        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "published")]
        public bool Published { get; set; }

        [DataMember(Name = "queryString")]
        public string QueryString { get; set; }

        [DataMember(Name = "target")]
        public string Target { get; set; }

        [DataMember(Name = "trashed")]
        public bool Trashed { get; set; }

        [DataMember(Name = "udi")]
        public GuidUdi Udi { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
