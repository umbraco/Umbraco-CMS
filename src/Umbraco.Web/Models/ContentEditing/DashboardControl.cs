using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "control", Namespace = "")]
    public class DashboardControl
    {
        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "caption")]
        public string Caption { get; set; }
    }
}
