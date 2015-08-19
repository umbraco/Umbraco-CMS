using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentType", Namespace = "")]
    public class ContentTypeCompositionDisplay : ContentTypeBasic
    {
        public ContentTypeCompositionDisplay()
        {
            //initialize collections so at least their never null
            Groups = new List<PropertyGroupDisplay>();
            AllowedContentTypes = new List<int>();
            CompositeContentTypes = new List<string>();
            AvailableCompositeContentTypes = new List<EntityBasic>();
        }

        //name, alias, icon, thumb, desc, inherited from basic

        //List view
        [DataMember(Name = "isContainer")]
        public bool IsContainer { get; set; }

        [DataMember(Name = "listViewEditorName")]
        public string ListViewEditorName { get; set; }

        //Tabs
        [DataMember(Name = "groups")]
        public IEnumerable<PropertyGroupDisplay> Groups { get; set; }

        //Allowed child types
        [DataMember(Name = "allowedContentTypes")]
        public IEnumerable<int> AllowedContentTypes { get; set; }

        //Compositions
        [DataMember(Name = "compositeContentTypes")]
        public IEnumerable<string> CompositeContentTypes { get; set; }

        [DataMember(Name = "availableCompositeContentTypes")]
        public IEnumerable<EntityBasic> AvailableCompositeContentTypes { get; set; }

        [DataMember(Name = "allowAsRoot")]
        public bool AllowAsRoot { get; set; }
    }
}
