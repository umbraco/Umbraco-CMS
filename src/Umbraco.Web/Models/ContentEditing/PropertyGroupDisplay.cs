using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyGroup", Namespace = "")]
    public class PropertyGroupDisplay
    {
        public PropertyGroupDisplay()
        {
            Properties = new List<PropertyTypeDisplay>();
        }

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

        //Indicate if this tab was inherited
        [DataMember(Name = "inherited")]
        public bool Inherited { get; set; }

        [DataMember(Name = "contentTypeId")]
        public int ContentTypeId { get; set; }

        [DataMember(Name = "parentTabContentTypes")]
        public IEnumerable<int> ParentTabContentTypes { get; set; }

        [DataMember(Name = "parentTabContentTypeNames")]
        public IEnumerable<string> ParentTabContentTypeNames { get; set; }
    }
}
