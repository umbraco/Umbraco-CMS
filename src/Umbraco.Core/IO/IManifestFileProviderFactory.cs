using Microsoft.Extensions.FileProviders;

namespace Umbraco.Cms.Core.IO;

/// <summary>
///     Factory for creating <see cref="IFileProvider" /> instances for providing the package.manifest file.
/// </summary>
public interface IManifestFileProviderFactory : IFileProviderFactory
{
}
