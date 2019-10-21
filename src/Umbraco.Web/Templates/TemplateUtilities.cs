using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using File = System.IO.File;

namespace Umbraco.Web.Templates
{
    //NOTE: I realize there is only one class in this namespace but I'm pretty positive that there will be more classes in
    //this namespace once we start migrating and cleaning up more code.

    /// <summary>
    /// Utility class used for templates
    /// </summary>
    public static class TemplateUtilities
    {
        const string TemporaryImageDataAttribute = "data-tmpimg";

        internal static string ParseInternalLinks(string text, bool preview, UmbracoContext umbracoContext)
        {
            using (umbracoContext.ForcedPreview(preview)) // force for url provider
            {
                text = ParseInternalLinks(text, umbracoContext.UrlProvider);
            }

            return text;
        }

        /// <summary>
        /// Parses the string looking for the {localLink} syntax and updates them to their correct links.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="urlProvider"></param>
        /// <returns></returns>
        public static string ParseInternalLinks(string text, UrlProvider urlProvider) =>
            ParseInternalLinks(text, urlProvider, Current.UmbracoContext.MediaCache);

        // TODO: Replace mediaCache with media url provider
        internal static string ParseInternalLinks(string text, UrlProvider urlProvider, IPublishedMediaCache mediaCache)
        {
            if (urlProvider == null) throw new ArgumentNullException(nameof(urlProvider));
            if (mediaCache == null) throw new ArgumentNullException(nameof(mediaCache));

            // Parse internal links
            var tags = LocalLinkPattern.Matches(text);
            foreach (Match tag in tags)
            {
                if (tag.Groups.Count > 0)
                {
                    var id = tag.Groups[1].Value; //.Remove(tag.Groups[1].Value.Length - 1, 1);

                    //The id could be an int or a UDI
                    if (Udi.TryParse(id, out var udi))
                    {
                        var guidUdi = udi as GuidUdi;
                        if (guidUdi != null)
                        {
                            var newLink = "#";
                            if (guidUdi.EntityType == Constants.UdiEntityType.Document)
                                newLink = urlProvider.GetUrl(guidUdi.Guid);
                            else if (guidUdi.EntityType == Constants.UdiEntityType.Media)
                                newLink = mediaCache.GetById(guidUdi.Guid)?.Url;

                            if (newLink == null)
                                newLink = "#";

                            text = text.Replace(tag.Value, "href=\"" + newLink);
                        }
                    }

                    if (int.TryParse(id, out var intId))
                    {
                        var newLink = urlProvider.GetUrl(intId);
                        text = text.Replace(tag.Value, "href=\"" + newLink);
                    }
                }
            }

            return text;
        }


        // static compiled regex for faster performance
        private static readonly Regex LocalLinkPattern = new Regex(@"href=""[/]?(?:\{|\%7B)localLink:([a-zA-Z0-9-://]+)(?:\}|\%7D)",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex ResolveUrlPattern = new Regex("(=[\"\']?)(\\W?\\~(?:.(?![\"\']?\\s+(?:\\S+)=|[>\"\']))+.)[\"\']?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex ResolveImgPattern = new Regex(@"(<img[^>]*src="")([^""\?]*)([^""]*""[^>]*data-udi="")([^""]*)(""[^>]*>)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// The RegEx matches any HTML attribute values that start with a tilde (~), those that match are passed to ResolveUrl to replace the tilde with the application path.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>
        /// When used with a Virtual-Directory set-up, this would resolve all URLs correctly.
        /// The recommendation is that the "ResolveUrlsFromTextString" option (in umbracoSettings.config) is set to false for non-Virtual-Directory installs.
        /// </remarks>
        public static string ResolveUrlsFromTextString(string text)
        {
            if (Current.Configs.Settings().Content.ResolveUrlsFromTextString == false) return text;

            using (var timer = Current.ProfilingLogger.DebugDuration(typeof(IOHelper), "ResolveUrlsFromTextString starting", "ResolveUrlsFromTextString complete"))
            {
                // find all relative urls (ie. urls that contain ~)
                var tags = ResolveUrlPattern.Matches(text);
                Current.Logger.Debug(typeof(IOHelper), "After regex: {Duration} matched: {TagsCount}", timer.Stopwatch.ElapsedMilliseconds, tags.Count);
                foreach (Match tag in tags)
                {
                    var url = "";
                    if (tag.Groups[1].Success)
                        url = tag.Groups[1].Value;

                    // The richtext editor inserts a slash in front of the url. That's why we need this little fix
                    //                if (url.StartsWith("/"))
                    //                    text = text.Replace(url, ResolveUrl(url.Substring(1)));
                    //                else
                    if (String.IsNullOrEmpty(url) == false)
                    {
                        var resolvedUrl = (url.Substring(0, 1) == "/") ? IOHelper.ResolveUrl(url.Substring(1)) : IOHelper.ResolveUrl(url);
                        text = text.Replace(url, resolvedUrl);
                    }
                }
            }

            return text;
        }

        public static string CleanForXss(string text, params char[] ignoreFromClean)
        {
            return text.CleanForXss(ignoreFromClean);
        }

        /// <summary>
        /// Parses the string looking for Umbraco image tags and updates them to their up-to-date image sources.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>Umbraco image tags are identified by their data-udi attributes</remarks>
        public static string ResolveMediaFromTextString(string text)
        {
            // don't attempt to proceed without a context
            if (Current.UmbracoContext == null || Current.UmbracoContext.Media == null)
            {
                return text;
            }

            return ResolveImgPattern.Replace(text, match =>
            {
                // match groups:
                // - 1 = from the beginning of the image tag until src attribute value begins
                // - 2 = the src attribute value excluding the querystring (if present)
                // - 3 = anything after group 2 and before the data-udi attribute value begins
                // - 4 = the data-udi attribute value
                // - 5 = anything after group 4 until the image tag is closed
                var udi = match.Groups[4].Value;
                if(udi.IsNullOrWhiteSpace() || GuidUdi.TryParse(udi, out var guidUdi) == false)
                {
                    return match.Value;
                }
                var media = Current.UmbracoContext.Media.GetById(guidUdi.Guid);
                if(media == null)
                {
                    // image does not exist - we could choose to remove the image entirely here (return empty string),
                    // but that would leave the editors completely in the dark as to why the image doesn't show
                    return match.Value;
                }

                var url = media.Url;
                return $"{match.Groups[1].Value}{url}{match.Groups[3].Value}{udi}{match.Groups[5].Value}";
            });
        }

        /// <summary>
        /// Removes media urls from &lt;img&gt; tags where a data-udi attribute is present
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static string RemoveMediaUrlsFromTextString(string text)
            // see comment in ResolveMediaFromTextString for group reference
            => ResolveImgPattern.Replace(text, "$1$3$4$5");

        internal static string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, ILogger logger)
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
                        mediaFile = mediaService.CreateMedia(mediaItemName, Constants.System.Root, Constants.Conventions.MediaTypes.Image, userId);
                    else
                        mediaFile = mediaService.CreateMedia(mediaItemName, mediaParentFolder, Constants.Conventions.MediaTypes.Image, userId);

                    var fileInfo = new FileInfo(absoluteTempImagePath);

                    var fileStream = fileInfo.OpenReadWithRetry();
                    if (fileStream == null) throw new InvalidOperationException("Could not acquire file stream");
                    using (fileStream)
                    {
                        mediaFile.SetValue(contentTypeBaseServiceProvider, Constants.Conventions.Media.File, safeFileName, fileStream);
                    }

                    mediaService.Save(mediaFile, userId);

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
                var mediaTyped = Current.UmbracoHelper.Media(udi.Guid);
                var location = mediaTyped.Url;

                // Find the width & height attributes as we need to set the imageprocessor QueryString
                var width = img.GetAttributeValue("width", int.MinValue);
                var height = img.GetAttributeValue("height", int.MinValue);

                if(width != int.MinValue && height != int.MinValue)
                {
                    location = $"{location}?width={width}&height={height}&mode=max";
                }
                
                img.SetAttributeValue("src", location);

                // Remove the data attribute (so we do not re-process this)
                img.Attributes.Remove(TemporaryImageDataAttribute);

                // Add to the dictionary to avoid dupes
                if(uploadedImages.ContainsKey(tmpImgPath) == false)
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
                        logger.Error(typeof(TemplateUtilities), ex, "Could not delete temp file or folder {FileName}", absoluteTempImagePath);
                    }
                }
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }
    }
}
