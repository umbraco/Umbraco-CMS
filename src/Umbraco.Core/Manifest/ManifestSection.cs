using System.Runtime.Serialization;
using Umbraco.Cms.Core.Sections;

namespace Umbraco.Cms.Core.Manifest;

[DataContract(Name = "section", Namespace = "")]
public class ManifestSection : ISection
{
    [DataMember(Name = "alias")]
    public string Alias { get; set; } = string.Empty;

    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;
}
