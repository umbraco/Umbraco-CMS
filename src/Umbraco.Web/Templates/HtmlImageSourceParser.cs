using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Templates
{
    /// <summary>
    /// HTML image source parser.
    /// </summary>
    public class HtmlImageSourceParser
    {
        /// <summary>
        /// The image URL generator.
        /// </summary>
        protected readonly IImageUrlGenerator _imageUrlGenerator;

        /// <summary>
        /// The umbraco context accessor.
        /// </summary>
        protected readonly IUmbracoContextAccessor _umbracoContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlImageSourceParser" /> class.
        /// </summary>
        /// <param name="getMediaUrl">The get media URL.</param>
        /// <param name="imageUrlGenerator">The image URL generator.</param>
        public HtmlImageSourceParser(Func<Guid, string> getMediaUrl, IImageUrlGenerator imageUrlGenerator = null)
        {
            this._getMediaUrl = getMediaUrl;
            this._imageUrlGenerator = imageUrlGenerator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlImageSourceParser" /> class.
        /// </summary>
        /// <param name="umbracoContextAccessor">The umbraco context accessor.</param>
        public HtmlImageSourceParser(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// The resolve img pattern.
        /// </summary>
        protected static readonly Regex ResolveImgPattern = new Regex(@"(<img[^>]*src="")([^""\?]*)((?:\?[^""]*)?""[^>]*data-udi="")([^""]*)(""[^>]*>)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// The data udi attribute regex.
        /// </summary>
        protected static readonly Regex DataUdiAttributeRegex = new Regex(@"data-udi=\\?(?:""|')(?<udi>umb://[A-z0-9\-]+/[A-z0-9]+)\\?(?:""|')",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// The get media URL.
        /// </summary>
        protected Func<Guid, string> _getMediaUrl;

        /// <summary>
        /// Parses out media UDIs from an html string based on 'data-udi' html attributes
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public virtual IEnumerable<Udi> FindUdisFromDataAttributes(string text)
        {
            var matches = DataUdiAttributeRegex.Matches(text);
            if (matches.Count == 0)
                yield break;

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 2 && Udi.TryParse(match.Groups[1].Value, out var udi))
                    yield return udi;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlImageSourceParser" /> class.
        /// </summary>
        /// <param name="umbracoContextAccessor">The umbraco context accessor.</param>
        /// <param name="imageUrlGenerator">The image URL generator.</param>
        public HtmlImageSourceParser(IUmbracoContextAccessor umbracoContextAccessor, IImageUrlGenerator imageUrlGenerator)
            : this(umbracoContextAccessor)
        {
            this._imageUrlGenerator = imageUrlGenerator;
        }

        /// <summary>
        /// Parses the string looking for Umbraco image tags and updates them to their up-to-date image sources.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        /// <remarks>
        /// Umbraco image tags are identified by their data-udi attributes
        /// </remarks>
        public virtual string EnsureImageSources(string text)
        {
            if (_getMediaUrl == null)
                _getMediaUrl = (guid) => _umbracoContextAccessor.UmbracoContext.UrlProvider.GetMediaUrl(guid);

            return ResolveImgPattern.Replace(text, match =>
            {
                // match groups:
                // - 1 = from the beginning of the image tag until src attribute value begins
                // - 2 = the src attribute value excluding the querystring (if present)
                // - 3 = anything after group 2 and before the data-udi attribute value begins
                // - 4 = the data-udi attribute value
                // - 5 = anything after group 4 until the image tag is closed
                var udi = match.Groups[4].Value;
                if (udi.IsNullOrWhiteSpace() || GuidUdi.TryParse(udi, out var guidUdi) == false)
                {
                    return match.Value;
                }
                var mediaUrl = _getMediaUrl(guidUdi.Guid);
                if (mediaUrl == null)
                {
                    // image does not exist - we could choose to remove the image entirely here (return empty string),
                    // but that would leave the editors completely in the dark as to why the image doesn't show
                    return match.Value;
                }

                string attributes;
                if (this._imageUrlGenerator != null &&
                    this.ParseImageUrlGenerationOptions(mediaUrl, match.Value, out var imageUrlGenerationOptions))
                {
                    var endSrc = match.Groups[3].Value.IndexOf('"');
                    attributes = match.Groups[3].Value.Substring(endSrc);
                    mediaUrl = this._imageUrlGenerator.GetImageUrl(imageUrlGenerationOptions);
                }
                else
                {
                    attributes = match.Groups[3].Value;
                }

                return $"{match.Groups[1].Value}{mediaUrl}{attributes}{udi}{match.Groups[5].Value}";
            });
        }

        /// <summary>
        /// Removes media urls from &lt;img&gt; tags where a data-udi attribute is present
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public virtual string RemoveImageSources(string text)
            // see comment in ResolveMediaFromTextString for group reference
            => ResolveImgPattern.Replace(text, "$1$3$4$5");

        /// <summary>
        /// Parses the <see cref="ImageUrlGenerationOptions" /> from the specified <paramref name="imgTag" />.
        /// </summary>
        /// <param name="mediaUrl">The media URL.</param>
        /// <param name="imgTag">The img tag.</param>
        /// <returns>
        /// The parsed <see cref="ImageUrlGenerationOptions" /> from the specified <paramref name="imgTag" />.
        /// </returns>
        protected virtual bool ParseImageUrlGenerationOptions(string mediaUrl, string imgTag, out ImageUrlGenerationOptions options)
        {
            var html = new HtmlDocument();
            html.LoadHtml(imgTag);
            var img = html.DocumentNode.FirstChild;
            if (img?.Name == "img")
            {
                var result = new Lazy<ImageUrlGenerationOptions>(() => new ImageUrlGenerationOptions(mediaUrl));
                if (double.TryParse(img.Attributes["width"]?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var width))
                {
                    result.Value.Width = (int)Math.Round(width);
                }

                if (double.TryParse(img.Attributes["height"]?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
                {
                    result.Value.Height = (int)Math.Round(height);
                }

                if (img.Attributes["data-mode"]?.Value.IsNullOrWhiteSpace() == false)
                {
                    result.Value.ImageCropMode = img.Attributes["data-mode"].Value;
                }
                else if (result.IsValueCreated)
                {
                    result.Value.ImageCropMode = "max";
                }

                options = result.IsValueCreated ? result.Value : default;
            }
            else
            {
                options = default;
            }

            return options != default;
        }
    }
}
