using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Templates
{
    /// <summary>
    /// Utility class used to parse internal links
    /// </summary>
    public sealed class HtmlLocalLinkParser
    {
        private static readonly Regex LocalUdiPattern = new Regex(@"[/]?(?:\{|\%7B)localLink:(?<udi>[a-zA-Z0-9-://]+)(?:\}|\%7D)",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex LocalLinkPattern = new Regex(@"(?<prefix>href="")" + LocalUdiPattern,
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex LocalLinkMarkdownPattern = new Regex(@"(?:" + LocalLinkPattern + @")|(?:(?<prefix>\[[0-9]+\]:\s*)" + LocalUdiPattern + @")|(?:(?<prefix>\[[^\]]+\]\()" + LocalUdiPattern + @")",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public HtmlLocalLinkParser(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        internal IEnumerable<Udi> FindUdisFromLocalLinks(string text, bool markdown = false)
        {
            foreach ((int? intId, GuidUdi udi, string tagValue, string format) in FindLocalLinkIds(text, markdown))
            {
                if (udi != null)
                    yield return udi; // In v8, we only care abuot UDIs
            }
        }

        /// <summary>
        /// Parses the string looking for the {localLink} syntax and updates them to their correct links.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="preview"></param>
        /// <param name="markdown">Indicates the text in Markdown rather than HTML</param>
        /// <returns></returns>
        public string EnsureInternalLinks(string text, bool preview, bool markdown = false)
        {
            if (_umbracoContextAccessor.UmbracoContext == null)
                throw new InvalidOperationException("Could not parse internal links, there is no current UmbracoContext");

            if (!preview)
                return EnsureInternalLinksInternal(text, markdown);

            using (_umbracoContextAccessor.UmbracoContext.ForcedPreview(preview)) // force for URL provider
            {
                return EnsureInternalLinksInternal(text, markdown);
            }
        }

        /// <summary>
        /// Parses the string looking for the {localLink} syntax and updates them to their correct links.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string EnsureInternalLinks(string text)
        {
            return EnsureInternalLinksInternal(text, false);
        }

        /// <summary>
        /// Parses the string looking for the {localLink} syntax and updates them to their correct links.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="markdown">Indicates the text in Markdown rather than HTML</param>
        /// <returns></returns>
        private string EnsureInternalLinksInternal(string text, bool markdown)
        {
            if (_umbracoContextAccessor.UmbracoContext == null)
                throw new InvalidOperationException("Could not parse internal links, there is no current UmbracoContext");

            var urlProvider = _umbracoContextAccessor.UmbracoContext.UrlProvider;

            foreach((int? intId, GuidUdi udi, string tagValue, string format) in FindLocalLinkIds(text, markdown))
            {
                if (udi != null)
                {
                    var newLink = "#";
                    if (udi.EntityType == Constants.UdiEntityType.Document)
                        newLink = urlProvider.GetUrl(udi.Guid);
                    else if (udi.EntityType == Constants.UdiEntityType.Media)
                        newLink = urlProvider.GetMediaUrl(udi.Guid);

                    if (newLink == null)
                        newLink = "#";

                    text = text.Replace(tagValue, string.Format(format, newLink));
                }
                else if (intId.HasValue)
                {
                    var newLink = urlProvider.GetUrl(intId.Value);
                    text = text.Replace(tagValue, string.Format(format, newLink));
                }
            }

            return text;
        }

        private IEnumerable<(int? intId, GuidUdi udi, string tagValue, string format)> FindLocalLinkIds(string text, bool markdown)
        {
            // Parse internal links
            var tags = (markdown ? LocalLinkMarkdownPattern : LocalLinkPattern).Matches(text);
            foreach (Match tag in tags)
            {
                if (tag.Groups.Count > 0)
                {
                    var id = tag.Groups["udi"].Value; //.Remove(tag.Groups[1].Value.Length - 1, 1);
                    var prefix = tag.Groups["prefix"]?.Value;
                    var format = $"{prefix}{{0}}";
                    //The id could be an int or a UDI
                    if (Udi.TryParse(id, out var udi))
                    {
                        var guidUdi = udi as GuidUdi;
                        if (guidUdi != null)
                            yield return (null, guidUdi, tag.Value, format);
                    }

                    if (int.TryParse(id, out var intId))
                    {
                        yield return (intId, null, tag.Value, format);
                    }
                }
            }

        }
    }
}
