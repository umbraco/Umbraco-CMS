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
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Web.Common.ImageProcessors
{
    /// <inheritdoc />
    public class FileSystemImageProvider : IImageProvider
    {
        private readonly IFileProvider _fileProvider;
        private string _rootPath;
        private readonly FormatUtilities _formatUtilities;

        /// <summary>
        /// A match function used by the resolver to identify itself as the correct resolver to use.
        /// </summary>
        private Func<HttpContext, bool> _match;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemImageProvider" /> class.
        /// </summary>
        /// <param name="mediaFileManager">The media file manager.</param>
        /// <param name="hostingEnvironment">The hosting environment.</param>
        /// <param name="globalSettings">The global settings options.</param>
        /// <param name="formatUtilities">The format utilities.</param>
        public FileSystemImageProvider(MediaFileManager mediaFileManager, IHostingEnvironment hostingEnvironment, IOptionsMonitor<GlobalSettings> globalSettings, FormatUtilities formatUtilities)
        {
            _fileProvider = mediaFileManager.CreateFileProvider();

            GlobalSettingsOnChange(globalSettings.CurrentValue, hostingEnvironment);
            globalSettings.OnChange(o => GlobalSettingsOnChange(o, hostingEnvironment));

            _formatUtilities = formatUtilities;
        }

        /// <inheritdoc />
        public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.CommandOnly;

        /// <inheritdoc />
        public Func<HttpContext, bool> Match
        {
            get => _match ?? IsMatch;
            set => _match = value;
        }

        private void GlobalSettingsOnChange(GlobalSettings options, IHostingEnvironment hostingEnvironment)
            => _rootPath = hostingEnvironment.ToAbsolute(options.UmbracoMediaUrl);

        private bool IsMatch(HttpContext context)
            => context.Request.Path.StartsWithSegments(_rootPath, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        public bool IsValidRequest(HttpContext context)
            => _formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) != null;

        /// <inheritdoc />
        public Task<IImageResolver> GetAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(_rootPath, StringComparison.InvariantCultureIgnoreCase, out PathString subpath) == false ||
                _fileProvider.GetFileInfo(subpath) is not IFileInfo fileInfo ||
                fileInfo.Exists == false)
            {
                return Task.FromResult<IImageResolver>(null);
            }

            var metadata = new ImageMetadata(fileInfo.LastModified.UtcDateTime, fileInfo.Length);

            return Task.FromResult<IImageResolver>(new PhysicalFileSystemResolver(fileInfo, metadata));
        }
    }
}
