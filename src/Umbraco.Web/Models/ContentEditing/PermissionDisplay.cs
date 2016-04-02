using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "permission", Namespace = "")]
    public class PermissionDisplay
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        
        [DataMember(Name = "key")]
        public string Key { get; set; }
    }
}
