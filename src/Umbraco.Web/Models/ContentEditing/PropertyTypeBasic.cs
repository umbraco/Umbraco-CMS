using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyType")]
    public class PropertyTypeBasic
    {
        //indicates if this property was inherited
        [DataMember(Name = "inherited")]
        public bool Inherited { get; set; }

        //TODO: Required ?
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^([a-zA-Z]\w.*)$", ErrorMessage = "Invalid alias")]
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "validation")]
        public PropertyTypeValidation Validation { get; set; }

        [DataMember(Name = "label")]
        [Required]
        public string Label { get; set; }

        [DataMember(Name = "sortOrder")]
        public int SortOrder { get; set; }

        [DataMember(Name = "dataTypeId")]
        [Required]
        public int DataTypeId { get; set; }

        //SD: Is this really needed ?
        [DataMember(Name = "groupId")]
        public int GroupId { get; set; }
    }
}