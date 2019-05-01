using System;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Templates
{
    //NOTE: I realize there is only one class in this namespace but I'm pretty positive that there will be more classes in
    //this namespace once we start migrating and cleaning up more code.

    /// <summary>
    /// Utility class used for templates
    /// </summary>
    public static class TemplateUtilities
    {
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
            if (Current.UmbracoContext == null || Current.UmbracoContext.MediaCache == null)
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
                var src = match.Groups[2].Value;
                var udi = match.Groups[4].Value;
                if(src.IsNullOrWhiteSpace() || udi.IsNullOrWhiteSpace() || GuidUdi.TryParse(udi, out var guidUdi) == false)
                {
                    return match.Value;
                }
                var media = Current.UmbracoContext.MediaCache.GetById(guidUdi.Guid);
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
    }
}
