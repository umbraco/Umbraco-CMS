using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "usertype", Namespace = "")]
    public class UserTypeBasic
    {
        [DataMember(Name = "alias")]
        public string Alias { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "permissions")]
        public IEnumerable<string> Permissions { get; set; }

        [DataMember(Name = "id")]
        public int Id { get; set; }
    }
}
