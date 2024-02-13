namespace Umbraco.Cms.Core.Manifest;

public class PackageManifestImportmap
{
    public required Dictionary<string, string> Imports { get; set; }
    public Dictionary<string, object>? Scopes { get; set; }
}
