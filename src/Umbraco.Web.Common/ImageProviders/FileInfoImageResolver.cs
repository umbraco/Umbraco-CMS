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
        private readonly ImageMetadata _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfoImageResolver"/> class.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <param name="metadata">The image metadata associated with this file.</param>
        public FileInfoImageResolver(IFileInfo fileInfo, in ImageMetadata metadata)
        {
            _fileInfo = fileInfo;
            _metadata = metadata;
        }

        /// <inheritdoc/>
        public Task<ImageMetadata> GetMetaDataAsync() => Task.FromResult(_metadata);

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult(_fileInfo.CreateReadStream());
    }
}
