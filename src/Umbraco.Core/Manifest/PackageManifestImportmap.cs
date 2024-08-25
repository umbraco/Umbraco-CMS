using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Manifest;

[DataContract(Name = "packageManifestImportmap", Namespace = "")]
public class PackageManifestImportmap
{
    [DataMember(Name = "imports")]
    public required Dictionary<string, string> Imports { get; set; }

    [DataMember(Name = "scopes")]
    public Dictionary<string, Dictionary<string, string>>? Scopes { get; set; }
}
