namespace Umbraco.Cms.Core.Manifest;

public interface IPackageManifestService
{
    Task<IEnumerable<PackageManifest>> GetPackageManifestsAsync();
}
