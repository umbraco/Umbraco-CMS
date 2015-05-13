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

        [DataMember(Name = "groups")]
        public IEnumerable<PropertyTypeGroupDisplay> Groups { get; set; }
    }
}
