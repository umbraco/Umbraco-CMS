using System.Runtime.Serialization;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Core.Manifest
{
    [DataContract(Name = "section", Namespace = "")]
    public class ManifestBackOfficeSection : IBackOfficeSection
    {
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
