using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.ImageProcessors
{
    public class UmbracoImageProvider : IImageProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly FormatUtilities _formatUtilities;
        private readonly string _mediaUrlPrefix;

        public UmbracoImageProvider(IUmbracoMediaFileProvider fileProvider, FormatUtilities formatUtilities, IOptions<GlobalSettings> globalSettings)
        {
            _fileProvider = fileProvider;
            _formatUtilities = formatUtilities;
            _mediaUrlPrefix = globalSettings.Value.UmbracoMediaUrl.TrimStart(Core.Constants.CharArrays.Tilde);
        }

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context) => _formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) != null;

        /// <inheritdoc/>
        public Task<IImageResolver> GetAsync(HttpContext context)
        {
            // Path has already been correctly parsed before here.
            IFileInfo fileInfo = _fileProvider.GetFileInfo(context.Request.Path.Value.TrimStart(_mediaUrlPrefix));

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return Task.FromResult<IImageResolver>(null);
            }

            var metadata = new ImageMetadata(fileInfo.LastModified.UtcDateTime, fileInfo.Length);
            return Task.FromResult<IImageResolver>(new PhysicalFileSystemResolver(fileInfo, metadata));
        }

        /// <inheritdoc/>
        public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.CommandOnly;

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;
    }
}
