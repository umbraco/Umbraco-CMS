using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentType", Namespace = "")]
    public class ContentTypeDisplay : ContentTypeBasic
    {
        //name, alias, icon, thumb, desc, inherited from basic


        // Templates
        [DataMember(Name = "allowedTemplates")]
        public IEnumerable<EntityBasic> AllowedTemplates { get; set; } 

        [DataMember(Name = "availableTemplates")]
        public IEnumerable<EntityBasic> AvailableTemplates { get; set; }

        [DataMember(Name = "defaultTemplate")]
        public string DefaultTemplate { get; set; }


        // Allowed parent node types (can include root)
        [DataMember(Name = "allowedParentNodeTypes")]
        public IEnumerable<EntityBasic> AllowedParentNodeTypes { get; set; }


        //List view
         [DataMember(Name = "enableListView")]
        public bool EnableListView { get; set; }


        //Compositions
        [DataMember(Name = "compositedContentTypes")]
        public IEnumerable<EntityBasic> CompositedContentTypes { get; set; }

        [DataMember(Name = "availableContentTypes")]
        public IEnumerable<EntityBasic> AvailableContentTypes { get; set; }


        //Tabs

        [DataMember(Name = "groups")]
        public IEnumerable<PropertyTypeGroupDisplay> Groups { get; set; }
    }
}
