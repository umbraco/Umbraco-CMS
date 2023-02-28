using Microsoft.Extensions.FileProviders;

namespace Umbraco.Cms.Core.IO;

/// <summary>
///     Factory for creating <see cref="IFileProvider" /> instances for providing the umbraco-package.json file.
/// </summary>
public interface IPackageManifestFileProviderFactory : IFileProviderFactory
{
}
