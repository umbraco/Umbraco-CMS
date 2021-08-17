using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentTypes", Namespace = "")]
    public class ContentTypesByAliases
    {
        [DataMember(Name = "parentId")]
        [Required]
        public int ParentId { get; set; }

        [DataMember(Name = "contentTypeAliases")]
        [Required]
        public string[] ContentTypeAliases { get; set; }
    }
}
