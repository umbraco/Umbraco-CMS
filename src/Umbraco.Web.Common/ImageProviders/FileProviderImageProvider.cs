using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Umbraco.Cms.Web.Common.ImageProviders
{
    /// <summary>
    /// Returns images from an <see cref="IFileProvider"/> abstraction.
    /// </summary>
    public class FileProviderImageProvider : IImageProvider
    {
        /// <summary>
        /// The file provider abstraction.
        /// </summary>
        private readonly IFileProvider _fileProvider;

        /// <summary>
        /// Contains various format helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities _formatUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProviderImageProvider"/> class.
        /// </summary>
        /// <param name="fileProvider">The file provider.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public FileProviderImageProvider(IFileProvider fileProvider, FormatUtilities formatUtilities)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _formatUtilities = formatUtilities ?? throw new ArgumentNullException(nameof(formatUtilities));
        }

        /// <inheritdoc/>
        public ProcessingBehavior ProcessingBehavior { get; protected set; } = ProcessingBehavior.CommandOnly;

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public virtual bool IsValidRequest(HttpContext context)
            => _formatUtilities.TryGetExtensionFromUri(context.Request.GetDisplayUrl(), out _);

        /// <inheritdoc/>
        public virtual Task<IImageResolver> GetAsync(HttpContext context)
        {
            IFileInfo fileInfo = _fileProvider.GetFileInfo(context.Request.Path);
            if (!fileInfo.Exists)
            {
                return Task.FromResult<IImageResolver>(null);
            }

            var metadata = new ImageMetadata(fileInfo.LastModified.UtcDateTime, fileInfo.Length);

            return Task.FromResult<IImageResolver>(new FileInfoImageResolver(fileInfo, metadata));
        }
    }
}
