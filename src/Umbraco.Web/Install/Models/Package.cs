using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Install.Models
{
    [DataContract(Name = "package")]
    public class Package
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "thumbnail")]
        public string Thumbnail { get; set; }
        [DataMember(Name = "id")]
        public Guid Id { get; set; }
    }
}
