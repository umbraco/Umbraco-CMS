using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "scriptFile", Namespace = "")]
    public class CodeFileDisplay
    {
        [DataMember(Name = "virtualPath")]
        public string VirtualPath { get; set; }
        [DataMember(Name = "content")]
        public string Content { get; set; }
        [DataMember(Name = "snippet")]
        public string Snippet { get; set; }
    }
}
