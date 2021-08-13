using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Media
{
    /// <summary>
    /// Exposes a method that generates an image URL based on the specified options that can be processed by ImageSharp.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Media.IImageUrlGenerator" />
    public class ImageSharpImageUrlGenerator : IImageUrlGenerator
    {
        private static readonly string[] s_supportedImageFileTypes = Configuration.Default.ImageFormats.SelectMany(f => f.FileExtensions).ToArray();

        /// <inheritdoc />
        /// <remarks>
        /// This uses the default instance of the ImageSharp configuration, so we need to ensure we don't new up a different instance when configuring the middleware.
        /// </remarks>
        public IEnumerable<string> SupportedImageFileTypes { get; } = s_supportedImageFileTypes;

        /// <inheritdoc/>
        public string GetImageUrl(ImageUrlGenerationOptions options)
        {
            if (options == null)
            {
                return null;
            }

            var imageUrl = new StringBuilder(options.ImageUrl);

            bool queryStringHasStarted = false;
            void AppendQueryString(string value)
            {
                imageUrl.Append(queryStringHasStarted ? '&' : '?');
                queryStringHasStarted = true;

                imageUrl.Append(value);
            }
            void AddQueryString(string key, params IConvertible[] values)
                => AppendQueryString(key + '=' + string.Join(",", values.Select(x => x.ToString(CultureInfo.InvariantCulture))));

            if (options.FocalPoint != null)
            {
                AddQueryString("rxy", options.FocalPoint.Left, options.FocalPoint.Top);
            }

            if (options.Crop != null)
            {
                AddQueryString("cc", options.Crop.Left, options.Crop.Top, options.Crop.Right, options.Crop.Bottom);
            }

            if (options.ImageCropMode.HasValue)
            {
                AddQueryString("rmode", options.ImageCropMode.Value.ToString().ToLowerInvariant());
            }

            if (options.ImageCropAnchor.HasValue)
            {
                AddQueryString("ranchor", options.ImageCropAnchor.Value.ToString().ToLowerInvariant());
            }

            if (options.Width.HasValue)
            {
                AddQueryString("width", options.Width.Value);
            }

            if (options.Height.HasValue)
            {
                AddQueryString("height", options.Height.Value);
            }

            if (options.Quality.HasValue)
            {
                AddQueryString("quality", options.Quality.Value);
            }

            if (string.IsNullOrWhiteSpace(options.FurtherOptions) == false)
            {
                AppendQueryString(options.FurtherOptions.TrimStart('?', '&'));
            }

            if (string.IsNullOrWhiteSpace(options.CacheBusterValue) == false)
            {
                AddQueryString("rnd", options.CacheBusterValue);
            }

            return imageUrl.ToString();
        }
    }
}
