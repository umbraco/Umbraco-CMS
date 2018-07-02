using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "stylesheetRule", Namespace = "")]
    public class StylesheetRule
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "selector")]
        public string Selector { get; set; }

    }
}
