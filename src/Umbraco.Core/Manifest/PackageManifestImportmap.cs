namespace Umbraco.Cms.Core.Manifest;

public class PackageManifestImportmap
{
    public required Dictionary<string, string> Imports { get; set; }
    public Dictionary<string, Dictionary<string, string>>? Scopes { get; set; }
}
