using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Umbraco.Cms.Core.IO
{
    /// <summary>
    /// Represents a directory in an <see cref="IFileSystem" />.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.FileProviders.IFileInfo" />
    public class FileSystemDirectoryInfo : IFileInfo
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _subpath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemDirectoryInfo" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="subpath">The subpath.</param>
        public FileSystemDirectoryInfo(IFileSystem fileSystem, string subpath)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _subpath = subpath ?? throw new ArgumentNullException(nameof(subpath));
        }

        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        public bool IsDirectory => true;

        /// <inheritdoc />
        public DateTimeOffset LastModified => _fileSystem.GetLastModified(_subpath);

        /// <inheritdoc />
        public long Length => -1;

        /// <inheritdoc />
        public string Name => _fileSystem.GetRelativePath(_subpath);

        /// <inheritdoc />
        public string PhysicalPath => null!;

        /// <inheritdoc />
        public Stream CreateReadStream() => throw new InvalidOperationException("Cannot create a stream for a directory.");
    }
}
