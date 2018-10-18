﻿using HtmlAgilityPack;
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Composing;
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
        public static string ParseInternalLinks(string text, UrlProvider urlProvider)
        {
            if (urlProvider == null) throw new ArgumentNullException("urlProvider");

            if(string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Parse internal links
            var tags = LocalLinkPattern.Matches(text);
            foreach (Match tag in tags)
            {
                if (tag.Groups.Count > 0)
                {
                    var id = tag.Groups[1].Value; //.Remove(tag.Groups[1].Value.Length - 1, 1);

                    //The id could be an int or a UDI
                    Udi udi;
                    if (Udi.TryParse(id, out udi))
                    {
                        var guidUdi = udi as GuidUdi;
                        if (guidUdi != null)
                        {
                            var newLink = urlProvider.GetUrl(guidUdi.Guid);
                            text = text.Replace(tag.Value, "href=\"" + newLink);
                        }
                    }
                    int intId;
                    if (int.TryParse(id, out intId))
                    {
                        var newLink = urlProvider.GetUrl(intId);
                        text = text.Replace(tag.Value, "href=\"" + newLink);
                    }
                }
            }

            if (UmbracoConfig.For.UmbracoSettings().Content.StripUdiAttributes)
            {
                text = StripUdiDataAttributes(text);
            }
            
            return text;
        }


        // static compiled regex for faster performance
        private static readonly Regex LocalLinkPattern = new Regex(@"href=""[/]?(?:\{|\%7B)localLink:([a-zA-Z0-9-://]+)(?:\}|\%7D)",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex ResolveUrlPattern = new Regex("(=[\"\']?)(\\W?\\~(?:.(?![\"\']?\\s+(?:\\S+)=|[>\"\']))+.)[\"\']?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex UdiDataAttributePattern = new Regex("data-udi=\"[^\\\"]*\"",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            if (UmbracoConfig.For.UmbracoSettings().Content.ResolveUrlsFromTextString == false) return text;

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
        /// Strips data-udi attributes from rich text
        /// </summary>
        /// <param name="input">A html string</param>
        /// <returns>A string stripped from the data-uid attributes</returns>
        public static string StripUdiDataAttributes(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }


            return UdiDataAttributePattern.Replace(input, string.Empty);
        }
    }
}
