namespace Umbraco.Cms.Api.Management.ViewModels.Package;

public class PackageManifestResponseModel
{
    public string Name { get; set; } = string.Empty;

    public string? Version { get; set; }

    public object[] Extensions { get; set; } = Array.Empty<object>();
}
