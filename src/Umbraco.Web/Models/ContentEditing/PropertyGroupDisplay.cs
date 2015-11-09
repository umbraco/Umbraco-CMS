using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyGroup", Namespace = "")]
    public class PropertyGroupDisplay : PropertyGroupBasic<PropertyTypeDisplay>
    {
        public PropertyGroupDisplay()
        {
            Properties = new List<PropertyTypeDisplay>();
            ParentTabContentTypeNames = new List<string>();
            ParentTabContentTypes = new List<int>();
        }
        
        [DataMember(Name = "parentGroupId")]
        [ReadOnly(true)]
        public int ParentGroupId { get; set; }
        
        //SD: Seems strange that this is required
        [DataMember(Name = "contentTypeId")]
        [ReadOnly(true)]
        public int ContentTypeId { get; set; }

        [DataMember(Name = "parentTabContentTypes")]
        [ReadOnly(true)]
        public IEnumerable<int> ParentTabContentTypes { get; set; }

        [DataMember(Name = "parentTabContentTypeNames")]
        [ReadOnly(true)]
        public IEnumerable<string> ParentTabContentTypeNames { get; set; }
    }
}
