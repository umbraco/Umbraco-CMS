using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "userGroup", Namespace = "")]
    public class UserGroupDisplay : UserGroupBasic
    {
        public UserGroupDisplay()
        {
            Users = Enumerable.Empty<UserBasic>();
        }

        [DataMember(Name = "users")]
        public IEnumerable<UserBasic> Users { get; set; }
    }
}