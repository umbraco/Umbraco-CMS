using Microsoft.Extensions.FileProviders;

namespace Umbraco.Cms.Core.IO;

/// <summary>
///     Factory for creating <see cref="IFileProvider" /> instances for providing the grid.editors.config.js file.
/// </summary>
public interface IGridEditorsConfigFileProviderFactory : IFileProviderFactory
{
}
