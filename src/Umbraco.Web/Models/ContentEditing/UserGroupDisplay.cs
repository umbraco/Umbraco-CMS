using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "userGroup", Namespace = "")]
    public class UserGroupDisplay : UserGroupBasic
    {
        [DataMember(Name = "users")]
        public IEnumerable<UserBasic> Users { get; set; }
    }
}