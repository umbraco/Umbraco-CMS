using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    public abstract class TabbedContentItem<T, TPersisted> : ContentItemBasic<T, TPersisted> 
        where T : ContentPropertyBasic 
        where TPersisted : IContentBase
    {
        protected TabbedContentItem()
        {
            Tabs = new List<Tab<T>>();
        }        
        
        /// <summary>
        /// Defines the tabs containing display properties
        /// </summary>
        [DataMember(Name = "tabs")]
        public IEnumerable<Tab<T>> Tabs { get; set; }

        /// <summary>
        /// Override the properties property to ensure we don't serialize this
        /// and to simply return the properties based on the properties in the tabs collection
        /// </summary>
        /// <remarks>
        /// This property cannot be set
        /// </remarks>
        [JsonIgnore]
        public override IEnumerable<T> Properties
        {
            get { return Tabs.SelectMany(x => x.Properties); }
            set { throw new NotImplementedException(); }
        }
    }
}