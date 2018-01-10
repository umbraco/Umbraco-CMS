using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "tour", Namespace = "")]
    public class BackOfficeTour
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "alias")]
        public string Alias { get; set; }
        [DataMember(Name = "group")]
        public string Group { get; set; }
        [DataMember(Name = "allowDisable")]
        public bool AllowDisable { get; set; }
        [DataMember(Name = "steps")]
        public BackOfficeTourStep[] Steps { get; set; }
    }
}
