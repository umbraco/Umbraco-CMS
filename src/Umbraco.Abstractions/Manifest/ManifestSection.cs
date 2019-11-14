using System.Runtime.Serialization;
using Umbraco.Core.Models.Sections;

namespace Umbraco.Core.Manifest
{
    [DataContract(Name = "section", Namespace = "")]
    public class ManifestSection : ISection
    {
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
