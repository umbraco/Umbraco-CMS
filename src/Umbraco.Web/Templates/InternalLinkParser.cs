using System;
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
    public sealed class InternalLinkParser
    {

        private static readonly Regex LocalLinkPattern = new Regex(@"href=""[/]?(?:\{|\%7B)localLink:([a-zA-Z0-9-://]+)(?:\}|\%7D)",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public InternalLinkParser(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public string ParseInternalLinks(string text, bool preview)
        {
            if (_umbracoContextAccessor.UmbracoContext == null)
                throw new InvalidOperationException("Could not parse internal links, there is no current UmbracoContext");

            if (!preview)
                return ParseInternalLinks(text);

            using (_umbracoContextAccessor.UmbracoContext.ForcedPreview(preview)) // force for url provider
            {
                return ParseInternalLinks(text);
            }
        }

        /// <summary>
        /// Parses the string looking for the {localLink} syntax and updates them to their correct links.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="urlProvider"></param>
        /// <returns></returns>
        public string ParseInternalLinks(string text)
        {
            if (_umbracoContextAccessor.UmbracoContext == null)
                throw new InvalidOperationException("Could not parse internal links, there is no current UmbracoContext");

            var urlProvider = _umbracoContextAccessor.UmbracoContext.UrlProvider;

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
                                newLink = urlProvider.GetMediaUrl(guidUdi.Guid);

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
    }
}
