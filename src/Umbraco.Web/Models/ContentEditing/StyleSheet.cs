using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "stylesheet", Namespace = "")]
    public class Stylesheet
    {
        [DataMember(Name="name")]
        public string Name { get; set; }
        
        [DataMember(Name = "path")]
        public string Path { get; set; }
    }
}
