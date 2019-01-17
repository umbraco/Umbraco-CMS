using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be displayed in the back office
    /// </summary>    
    [DataContract(Name = "content", Namespace = "")]
    public class ContentItemDisplay : ListViewAwareContentItemDisplayBase<ContentPropertyDisplay, IContent>
    {
        public ContentItemDisplay()
        {
            AllowPreview = true;
        }

        [DataMember(Name = "publishDate")]
        public DateTime? PublishDate { get; set; }

        [DataMember(Name = "releaseDate")]
        public DateTime? ReleaseDate { get; set; }

        [DataMember(Name = "removeDate")]
        public DateTime? ExpireDate { get; set; }

        [DataMember(Name = "template")]
        public string TemplateAlias { get; set; }




        [DataMember(Name = "templateId")]
        public int TemplateId { get; set; }




        [DataMember(Name = "allowedTemplates")]
        public IDictionary<string, string> AllowedTemplates { get; set; }

        [DataMember(Name = "documentType")]
        public ContentTypeBasic DocumentType { get; set; }

        [DataMember(Name = "urls")]
        public string[] Urls { get; set; }

        /// <summary>
        /// Determines whether previewing is allowed for this node
        /// </summary>
        /// <remarks>
        /// By default this is true but by using events developers can toggle this off for certain documents if there is nothing to preview
        /// </remarks>
        [DataMember( Name = "allowPreview" )]
        public bool AllowPreview { get; set; }

        /// <summary>
        /// The allowed 'actions' based on the user's permissions - Create, Update, Publish, Send to publish
        /// </summary>
        /// <remarks>
        /// Each char represents a button which we can then map on the front-end to the correct actions
        /// </remarks>
        [DataMember(Name = "allowedActions")]
        public IEnumerable<string> AllowedActions { get; set; }

        [DataMember(Name = "isBlueprint")]
        public bool IsBlueprint { get; set; }
    }
}
