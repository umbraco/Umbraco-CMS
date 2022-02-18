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
        /// <inheritdoc />
        public IEnumerable<string> SupportedImageFileTypes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
        /// </summary>
        /// <param name="configuration">The ImageSharp configuration.</param>
        public ImageSharpImageUrlGenerator(Configuration configuration)
            : this(configuration.ImageFormats.SelectMany(f => f.FileExtensions).ToArray())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
        /// </summary>
        /// <param name="supportedImageFileTypes">The supported image file types/extensions.</param>
        /// <remarks>
        /// This constructor is only used for testing.
        /// </remarks>
        internal ImageSharpImageUrlGenerator(IEnumerable<string> supportedImageFileTypes) => SupportedImageFileTypes = supportedImageFileTypes;

        /// <inheritdoc/>
        public string? GetImageUrl(ImageUrlGenerationOptions options)
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

            if (options.Crop != null)
            {
                AddQueryString("cc", options.Crop.Left, options.Crop.Top, options.Crop.Right, options.Crop.Bottom);
            }

            if (options.FocalPoint != null)
            {
                AddQueryString("rxy", options.FocalPoint.Left, options.FocalPoint.Top);
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
