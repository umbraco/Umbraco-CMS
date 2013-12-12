using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Validation;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.ContentEditing
{
   
    /// <summary>
    /// A model representing a content item to be displayed in the back office
    /// </summary>    
    [DataContract(Name = "content", Namespace = "")]
    public class ContentItemDisplay : ListViewAwareContentItemDisplayBase<ContentPropertyDisplay, IContent>
    {
        [DataMember(Name = "publishDate")]
        public DateTime? PublishDate { get; set; }

        [DataMember(Name = "releaseDate")]
        public DateTime? ReleaseDate { get; set; }

        [DataMember(Name = "removeDate")]
        public DateTime? ExpireDate { get; set; }

        [DataMember(Name = "template")]
        public string TemplateAlias { get; set; }

        [DataMember(Name = "urls")]
        public string[] Urls { get; set; }
        
        /// <summary>
        /// The allowed 'actions' based on the user's permissions - Create, Update, Publish, Send to publish
        /// </summary>
        /// <remarks>
        /// Each char represents a button which we can then map on the front-end to the correct actions
        /// </remarks>
        [DataMember(Name = "allowedActions")]
        public IEnumerable<char> AllowedActions { get; set; }

    }
}