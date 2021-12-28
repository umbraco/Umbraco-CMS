using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Umbraco.Cms.Core.IO
{
    /// <summary>
    /// Exposes an <see cref="IFileSystem" /> as an <see cref="IFileProvider" />.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.FileProviders.IFileProvider" />
    public class FileSystemFileProvider : IFileProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemFileProvider" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="pathPrefix">The path prefix.</param>
        /// <exception cref="System.ArgumentNullException">fileSystem</exception>
        public FileSystemFileProvider(IFileSystem fileSystem, string pathPrefix = null)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            PathPrefix = pathPrefix;
        }

        /// <summary>
        /// Gets the file system.
        /// </summary>
        protected IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the path prefix.
        /// </summary>
        protected string PathPrefix { get; }

        /// <inheritdoc />
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var path = PathPrefix + subpath;
            if (path == null || !FileSystem.DirectoryExists(path))
            {
                return NotFoundDirectoryContents.Singleton;
            }

            return new FileSystemDirectoryContents(FileSystem, path);
        }

        /// <inheritdoc />
        public IFileInfo GetFileInfo(string subpath)
        {
            var path = PathPrefix + subpath;
            if (path == null || !FileSystem.FileExists(path))
            {
                return new NotFoundFileInfo(path);
            }

            return new FileSystemFileInfo(FileSystem, path);
        }

        /// <inheritdoc />
        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
    }
}
