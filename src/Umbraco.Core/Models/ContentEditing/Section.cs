﻿using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a section (application) in the back office
    /// </summary>
    [DataContract(Name = "section", Namespace = "")]
    public class Section
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        /// <summary>
        /// In some cases a custom route path can be specified so that when clicking on a section it goes to this
        /// path instead of the normal dashboard path
        /// </summary>
        [DataMember(Name = "routePath")]
        public string RoutePath { get; set; }
    }
}
