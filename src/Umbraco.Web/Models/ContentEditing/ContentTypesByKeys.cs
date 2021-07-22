using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentTypes", Namespace = "")]
    public class ContentTypesByKeys
    {
        [DataMember(Name = "parentId")]
        [Required]
        public int ParentId { get; set; }

        [DataMember(Name = "contentTypeKeys")]
        [Required]
        public Guid[] ContentTypeKeys { get; set; }
    }
}
