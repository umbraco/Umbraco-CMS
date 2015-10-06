using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyGroup", Namespace = "")]
    public class PropertyGroupBasic<TPropertyType>
        where TPropertyType: PropertyTypeBasic
    {
        public PropertyGroupBasic()
        {
            Properties = new List<TPropertyType>();
        }

        //Indicate if this tab was inherited
        [DataMember(Name = "inherited")]
        public bool Inherited { get; set; }

        //TODO: Required ?
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "properties")]
        public IEnumerable<TPropertyType> Properties { get; set; }

        [DataMember(Name = "sortOrder")]
        public int SortOrder { get; set; }

        [Required]
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}