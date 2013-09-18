using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{    
    public class UserPasswordChange
    {
        [DataMember(Name = "oldPassword", IsRequired = true)]
        [Required]
        public string OldPassword { get; set; }

        [DataMember(Name = "newPassword", IsRequired = true)]
        [Required]
        public string NewPassword { get; set; }
    }
}
