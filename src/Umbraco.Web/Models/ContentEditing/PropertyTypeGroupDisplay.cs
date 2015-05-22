using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyTypeGroup", Namespace = "")]
    public class PropertyTypeGroupDisplay
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "parentGroupId")]
        public int ParentGroupId { get; set; }

        [DataMember(Name = "sortOrder")]
        public int SortOrder { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "properties")]
        public IEnumerable<PropertyTypeDisplay> Properties { get; set; }

        //TODO: Why is there recursive list of PropertyTypeGroupDisplay? Not sure how this
        // is intended to work but seems like it will become very complicated. This will also 
        // mean that serialization won't work very well because you cannot serialize a recursive 
        // property. These models should just be flat lists of properties and groups with properties
        // indicating where they've come from. These models don't have to be an exact representation
        // of their data structures, they should be structured in the simplest format in order for
        // us to pass data to and from the editor, and that's it.
       // [DataMember(Name = "groups")]
       // public IEnumerable<PropertyTypeGroupDisplay> Groups { get; set; }
        
        //Indicate if this tab was inherited
        [DataMember(Name = "inherited")]
        public bool Inherited { get; set; }


        [DataMember(Name = "parent")]
        public EntityBasic Parent { get; set; }

    }
}
