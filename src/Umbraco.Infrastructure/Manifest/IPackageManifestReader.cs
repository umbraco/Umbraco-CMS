using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Infrastructure.Manifest;

public interface IPackageManifestReader
{
    Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync();
}
