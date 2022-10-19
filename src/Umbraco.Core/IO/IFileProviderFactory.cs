using Microsoft.Extensions.FileProviders;

namespace Umbraco.Cms.Core.IO;

/// <summary>
///     Factory for creating <see cref="IFileProvider" /> instances.
/// </summary>
public interface IFileProviderFactory
{
    /// <summary>
    ///     Creates a new <see cref="IFileProvider" /> instance.
    /// </summary>
    /// <returns>
    ///     The newly created <see cref="IFileProvider" /> instance (or <c>null</c> if not supported).
    /// </returns>
    IFileProvider? Create();
}
