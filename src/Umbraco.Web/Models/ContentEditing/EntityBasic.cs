using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.Validation;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "entity", Namespace = "")]
    public class EntityBasic
    {
        [DataMember(Name = "name", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Name { get; set; }

        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }
        
        [DataMember(Name = "icon")]
        public string Icon { get; set; }
    }
}
