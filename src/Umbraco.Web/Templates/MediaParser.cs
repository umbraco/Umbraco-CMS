using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Templates
{

    public sealed class MediaParser
    {
        public MediaParser(IUmbracoContextAccessor umbracoContextAccessor, ILogger logger, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
            _mediaService = mediaService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        }

        private static readonly Regex ResolveImgPattern = new Regex(@"(<img[^>]*src="")([^""\?]*)([^""]*""[^>]*data-udi="")([^""]*)(""[^>]*>)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger _logger;
        private readonly IMediaService _mediaService;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        const string TemporaryImageDataAttribute = "data-tmpimg";

        /// <summary>
        /// Parses the string looking for Umbraco image tags and updates them to their up-to-date image sources.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>Umbraco image tags are identified by their data-udi attributes</remarks>
        public string EnsureImageSources(string text)
        {
            // don't attempt to proceed without a context
            if (_umbracoContextAccessor?.UmbracoContext?.UrlProvider == null)
            {
                return text;
            }

            var urlProvider = _umbracoContextAccessor.UmbracoContext.UrlProvider;

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
                var mediaUrl = urlProvider.GetMediaUrl(guidUdi.Guid);
                if (mediaUrl == null)
                {
                    // image does not exist - we could choose to remove the image entirely here (return empty string),
                    // but that would leave the editors completely in the dark as to why the image doesn't show
                    return match.Value;
                }

                return $"{match.Groups[1].Value}{mediaUrl}{match.Groups[3].Value}{udi}{match.Groups[5].Value}";
            });
        }

        /// <summary>
        /// Removes media urls from &lt;img&gt; tags where a data-udi attribute is present
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal string RemoveImageSources(string text)
            // see comment in ResolveMediaFromTextString for group reference
            => ResolveImgPattern.Replace(text, "$1$3$4$5");

        internal string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId)
        {
            // Find all img's that has data-tmpimg attribute
            // Use HTML Agility Pack - https://html-agility-pack.net
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var tmpImages = htmlDoc.DocumentNode.SelectNodes($"//img[@{TemporaryImageDataAttribute}]");
            if (tmpImages == null || tmpImages.Count == 0)
                return html;

            // An array to contain a list of URLs that
            // we have already processed to avoid dupes
            var uploadedImages = new Dictionary<string, GuidUdi>();

            foreach (var img in tmpImages)
            {
                // The data attribute contains the path to the tmp img to persist as a media item
                var tmpImgPath = img.GetAttributeValue(TemporaryImageDataAttribute, string.Empty);

                if (string.IsNullOrEmpty(tmpImgPath))
                    continue;

                var absoluteTempImagePath = IOHelper.MapPath(tmpImgPath);
                var fileName = Path.GetFileName(absoluteTempImagePath);
                var safeFileName = fileName.ToSafeFileName();

                var mediaItemName = safeFileName.ToFriendlyName();
                IMedia mediaFile;
                GuidUdi udi;

                if (uploadedImages.ContainsKey(tmpImgPath) == false)
                {
                    if (mediaParentFolder == Guid.Empty)
                        mediaFile = _mediaService.CreateMedia(mediaItemName, Constants.System.Root, Constants.Conventions.MediaTypes.Image, userId);
                    else
                        mediaFile = _mediaService.CreateMedia(mediaItemName, mediaParentFolder, Constants.Conventions.MediaTypes.Image, userId);

                    var fileInfo = new FileInfo(absoluteTempImagePath);

                    var fileStream = fileInfo.OpenReadWithRetry();
                    if (fileStream == null) throw new InvalidOperationException("Could not acquire file stream");
                    using (fileStream)
                    {
                        mediaFile.SetValue(_contentTypeBaseServiceProvider, Constants.Conventions.Media.File, safeFileName, fileStream);
                    }

                    _mediaService.Save(mediaFile, userId);

                    udi = mediaFile.GetUdi();
                }
                else
                {
                    // Already been uploaded & we have it's UDI
                    udi = uploadedImages[tmpImgPath];
                }

                // Add the UDI to the img element as new data attribute
                img.SetAttributeValue("data-udi", udi.ToString());

                // Get the new persisted image url
                var mediaTyped = _umbracoContextAccessor?.UmbracoContext?.Media.GetById(udi.Guid);
                if (mediaTyped == null)
                    throw new PanicException($"Could not find media by id {udi.Guid} or there was no UmbracoContext available.");

                var location = mediaTyped.Url;

                // Find the width & height attributes as we need to set the imageprocessor QueryString
                var width = img.GetAttributeValue("width", int.MinValue);
                var height = img.GetAttributeValue("height", int.MinValue);

                if (width != int.MinValue && height != int.MinValue)
                {
                    location = $"{location}?width={width}&height={height}&mode=max";
                }

                img.SetAttributeValue("src", location);

                // Remove the data attribute (so we do not re-process this)
                img.Attributes.Remove(TemporaryImageDataAttribute);

                // Add to the dictionary to avoid dupes
                if (uploadedImages.ContainsKey(tmpImgPath) == false)
                {
                    uploadedImages.Add(tmpImgPath, udi);

                    // Delete folder & image now its saved in media
                    // The folder should contain one image - as a unique guid folder created
                    // for each image uploaded from TinyMceController
                    var folderName = Path.GetDirectoryName(absoluteTempImagePath);
                    try
                    {
                        Directory.Delete(folderName, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(typeof(MediaParser), ex, "Could not delete temp file or folder {FileName}", absoluteTempImagePath);
                    }
                }
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }
    }
}
