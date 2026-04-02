using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Represents an import map configuration for a package manifest, used for JavaScript module resolution.
/// </summary>
[DataContract(Name = "packageManifestImportmap", Namespace = "")]
public class PackageManifestImportmap
{
    /// <summary>
    ///     Gets or sets the imports dictionary that maps module specifiers to URLs.
    /// </summary>
    [DataMember(Name = "imports")]
    public required Dictionary<string, string> Imports { get; set; }

    /// <summary>
    ///     Gets or sets the scopes dictionary that provides scoped import mappings for specific URL prefixes.
    /// </summary>
    [DataMember(Name = "scopes")]
    public Dictionary<string, Dictionary<string, string>>? Scopes { get; set; }
}
