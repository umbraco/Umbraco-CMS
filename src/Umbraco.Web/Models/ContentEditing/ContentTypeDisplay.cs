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
            AvailableCompositeContentTypes = new List<EntityBasic>();
            AllowedContentTypes = new List<EntityBasic>();
            CompositeContentTypes = new List<EntityBasic>();
            Groups = new List<PropertyTypeGroupDisplay>();
        }

        //name, alias, icon, thumb, desc, inherited from basic


        // Templates
        [DataMember(Name = "allowedTemplates")]
        public IEnumerable<EntityBasic> AllowedTemplates { get; set; } 

        [DataMember(Name = "defaultTemplate")]
        public EntityBasic DefaultTemplate { get; set; }

        // Allowed parent node types (could include root?)
        [DataMember(Name = "allowedContentTypes")]
        public IEnumerable<EntityBasic> AllowedContentTypes { get; set; }


        //List view
        [DataMember(Name = "enableListView")]
        public bool EnableListView { get; set; }

        //we might not need this... 
        [DataMember(Name = "allowedAtRoot")]
        public bool AllowedAsRoot { get; set; }

        //Compositions
        [DataMember(Name = "compositeContentTypes")]
        public IEnumerable<EntityBasic> CompositeContentTypes { get; set; }

        [DataMember(Name = "availableCompositeContentTypes")]
        public IEnumerable<EntityBasic> AvailableCompositeContentTypes { get; set; }


        //Tabs

        [DataMember(Name = "groups")]
        public IEnumerable<PropertyTypeGroupDisplay> Groups { get; set; }
    }
}
