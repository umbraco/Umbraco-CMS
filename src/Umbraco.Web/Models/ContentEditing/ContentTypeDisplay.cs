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
        public ContentTypeDisplay()
        {
            //initialize collections so at least their never null
            AllowedTemplates = new List<EntityBasic>();
            AvailableTemplates = new List<EntityBasic>();
            AvailableContentTypes = new List<EntityBasic>();
            AllowedParentNodeTypes = new List<EntityBasic>();
            CompositeContentTypes = new List<EntityBasic>();
            Groups = new List<PropertyTypeGroupDisplay>();
        }

        //name, alias, icon, thumb, desc, inherited from basic


        // Templates
        [DataMember(Name = "allowedTemplates")]
        public IEnumerable<EntityBasic> AllowedTemplates { get; set; } 

        [DataMember(Name = "availableTemplates")]
        public IEnumerable<EntityBasic> AvailableTemplates { get; set; }

        [DataMember(Name = "defaultTemplate")]
        public EntityBasic DefaultTemplate { get; set; }


        // Allowed parent node types (can include root)
        [DataMember(Name = "allowedParentNodeTypes")]
        public IEnumerable<EntityBasic> AllowedParentNodeTypes { get; set; }


        //List view
        [DataMember(Name = "enableListView")]
        public bool EnableListView { get; set; }

        [DataMember(Name = "allowedAtRoot")]
        public bool AllowedAsRoot { get; set; }

        //Compositions
        [DataMember(Name = "compositeContentTypes")]
        public IEnumerable<EntityBasic> CompositeContentTypes { get; set; }

        [DataMember(Name = "availableContentTypes")]
        public IEnumerable<EntityBasic> AvailableContentTypes { get; set; }


        //Tabs

        [DataMember(Name = "groups")]
        public IEnumerable<PropertyTypeGroupDisplay> Groups { get; set; }
    }
}
