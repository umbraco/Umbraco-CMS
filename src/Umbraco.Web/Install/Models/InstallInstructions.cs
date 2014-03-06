using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace Umbraco.Web.Install.Models
{
    [DataContract(Name = "installInstructions", Namespace = "")]
    public class InstallInstructions
    {
        [DataMember(Name = "instructions")]
        public IDictionary<string, JToken> Instructions { get; set; }

        [DataMember(Name = "installId")]
        public Guid InstallId { get; set; }
    }
}