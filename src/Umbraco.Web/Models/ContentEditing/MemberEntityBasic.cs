using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.Validation;

namespace Umbraco.Web.Models.ContentEditing
{
    public class MemberEntityBasic : EntityBasic
    {
        [DataMember(Name = "email", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Email { get; set; }

        [DataMember(Name = "loginName", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string LoginName { get; set; }
    }
}
