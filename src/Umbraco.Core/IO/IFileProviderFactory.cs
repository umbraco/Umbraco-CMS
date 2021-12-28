using Microsoft.Extensions.FileProviders;

namespace Umbraco.Cms.Core.IO
{
    /// <summary>
    /// Factory for creating <see cref="IFileProvider" /> instances.
    /// </summary>
    public interface IFileProviderFactory
    {
        /// <summary>
        /// Creates a new <see cref="IFileProvider" /> instance.
        /// </summary>
        /// <returns>
        /// The newly created <see cref="IFileProvider" /> instance.
        /// </returns>
        IFileProvider Create();
    }
}
