using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.ImageProcessors;
using static Umbraco.Cms.Core.Models.ImageUrlGenerationOptions;

namespace Umbraco.Cms.Web.Common.Media
{
    /// <summary>
    /// Exposes a method that generates an image URL based on the specified options that can be processed by ImageSharp.
    /// </summary>
    /// <seealso cref="IImageUrlGenerator" />
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
            if (options?.ImageUrl == null)
            {
                return null;
            }

            var queryString = new Dictionary<string, string?>();

            if (options.Crop is CropCoordinates crop)
            {
                queryString.Add(CropWebProcessor.Coordinates, FormattableString.Invariant($"{crop.Left},{crop.Top},{crop.Right},{crop.Bottom}"));
            }

            if (options.FocalPoint is FocalPointPosition focalPoint)
            {
                queryString.Add(ResizeWebProcessor.Xy, FormattableString.Invariant($"{focalPoint.Left},{focalPoint.Top}"));
            }

            if (options.ImageCropMode is ImageCropMode imageCropMode)
            {
                queryString.Add(ResizeWebProcessor.Mode, imageCropMode.ToString().ToLowerInvariant());
            }

            if (options.ImageCropAnchor is ImageCropAnchor imageCropAnchor)
            {
                queryString.Add(ResizeWebProcessor.Anchor, imageCropAnchor.ToString().ToLowerInvariant());
            }

            if (options.Width is int width)
            {
                queryString.Add(ResizeWebProcessor.Width, width.ToString(CultureInfo.InvariantCulture));
            }

            if (options.Height is int height)
            {
                queryString.Add(ResizeWebProcessor.Height, height.ToString(CultureInfo.InvariantCulture));
            }

            if (options.Quality is int quality)
            {
                queryString.Add(QualityWebProcessor.Quality, quality.ToString(CultureInfo.InvariantCulture));
            }

            foreach (KeyValuePair<string, StringValues> kvp in QueryHelpers.ParseQuery(options.FurtherOptions))
            {
                queryString.Add(kvp.Key, kvp.Value);
            }

            if (options.CacheBusterValue is string cacheBusterValue && !string.IsNullOrWhiteSpace(cacheBusterValue))
            {
                queryString.Add("rnd", cacheBusterValue);
            }

            return QueryHelpers.AddQueryString(options.ImageUrl, queryString);
        }
    }
}
