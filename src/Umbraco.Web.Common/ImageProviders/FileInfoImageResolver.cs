using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Umbraco.Cms.Web.Common.ImageProviders
{
    /// <summary>
    /// Provides means to manage image buffers from an <see cref="IImageInfo"/> instance.
    /// </summary>
    public class FileInfoImageResolver : IImageResolver
    {
        private readonly IFileInfo _fileInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfoImageResolver"/> class.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        public FileInfoImageResolver(IFileInfo fileInfo)
            => _fileInfo = fileInfo;

        /// <inheritdoc/>
        public Task<ImageMetadata> GetMetaDataAsync() => Task.FromResult(new ImageMetadata(_fileInfo.LastModified.UtcDateTime, _fileInfo.Length));

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult(_fileInfo.CreateReadStream());
    }
}
