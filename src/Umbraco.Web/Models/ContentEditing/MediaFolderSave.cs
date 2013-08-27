using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "media", Namespace = "")]
    public class MediaFolderSave
    {
        [DataMember(Name = "name", IsRequired = true)]
        [Required]
        public string Name { get; set; }

        [DataMember(Name = "parentId", IsRequired = true)]
        [Required]
        public int ParentId { get; set; }
    }
}
