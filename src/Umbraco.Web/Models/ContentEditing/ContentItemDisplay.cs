using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Linq;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be displayed in the back office
    /// </summary>    
    public class ContentItemDisplay : ContentItemBasic<ContentPropertyDisplay>
    {
        public ContentItemDisplay()
        {
            Tabs = new List<Tab<ContentPropertyDisplay>>();
        }

        [DataMember(Name = "name", IsRequired = true)]
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        /// <summary>
        /// Defines the tabs containing display properties
        /// </summary>
        [DataMember(Name = "tabs")]
        public IEnumerable<Tab<ContentPropertyDisplay>> Tabs { get; set; }

        /// <summary>
        /// Override the properties property to ensure we don't serialize this
        /// and to simply return the properties based on the properties in the tabs collection
        /// </summary>
        /// <remarks>
        /// This property cannot be set
        /// </remarks>
        [JsonIgnore]
        public override IEnumerable<ContentPropertyDisplay> Properties
        {
            get { return Tabs.SelectMany(x => x.Properties); }
            set { throw new NotImplementedException(); }
        }

    }
}