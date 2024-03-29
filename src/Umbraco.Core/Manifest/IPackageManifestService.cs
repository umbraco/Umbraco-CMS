namespace Umbraco.Cms.Core.Manifest;

public interface IPackageManifestService
{
    Task<IEnumerable<PackageManifest>> GetAllPackageManifestsAsync();

    Task<IEnumerable<PackageManifest>> GetPublicPackageManifestsAsync();

    Task<PackageManifestImportmap> GetPackageManifestImportmapAsync();
}
