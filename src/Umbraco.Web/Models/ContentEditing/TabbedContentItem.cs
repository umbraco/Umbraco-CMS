using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    public abstract class TabbedContentItem<T> : ContentItemBasic<T>, ITabbedContent<T> where T : ContentPropertyBasic
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

        // note
        // once a [DataContract] has been defined on a class, with a [DataMember] property,
        // one simply cannot ignore that property anymore - [IgnoreDataMember] on an overridden
        // property is ignored, and 'newing' the property means that it's the base property
        // which is used
        //
        // OTOH, Json.NET is happy having [JsonIgnore] on overrides, even though the base
        // property is [JsonProperty]. so, forcing [JsonIgnore] here, but really, we should
        // rethink the whole thing.

        /// <summary>
        /// Override the properties property to ensure we don't serialize this
        /// and to simply return the properties based on the properties in the tabs collection
        /// </summary>
        /// <remarks>
        /// This property cannot be set
        /// </remarks>
        [IgnoreDataMember]
        [JsonIgnore] // see note above on IgnoreDataMember vs JsonIgnore
        public override IEnumerable<T> Properties
        {
            get => Tabs.SelectMany(x => x.Properties);
            set => throw new NotImplementedException();
        }
    }
}
