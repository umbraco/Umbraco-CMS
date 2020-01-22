using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page aliases.
    /// </summary>
    /// <remarks>
    /// <para>Handles <c>/just/about/anything</c> where <c>/just/about/anything</c> is contained in the <c>umbracoUrlAlias</c> property of a document.</para>
    /// <para>The alias is the full path to the document. There can be more than one alias, separated by commas.</para>
    /// </remarks>
    public class ContentFinderByUrlAlias : IContentFinder
    {
        protected ILogger Logger { get; }

        public ContentFinderByUrlAlias(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TryFindContent(PublishedRequest frequest)
        {
            IPublishedContent node = null;

            if (frequest.Uri.AbsolutePath != "/") // no alias if "/"
            {
                node = FindContentByAlias(frequest.UmbracoContext.Content,
                    frequest.HasDomain ? frequest.Domain.ContentId : 0,
                    frequest.Culture.Name,
                    frequest.Uri.GetAbsolutePathDecoded(),
                    frequest.HasDomain ? frequest.Domain.Name : string.Empty);

                if (node != null)
                {
                    frequest.PublishedContent = node;
                    Logger.Debug<ContentFinderByUrlAlias>("Path '{UriAbsolutePath}' is an alias for id={PublishedContentId}", frequest.Uri.AbsolutePath, frequest.PublishedContent.Id);
                }
            }

            return node != null;
        }

        private static IPublishedContent FindContentByAlias(IPublishedContentCache cache, int rootNodeId, string culture, string alias, string domaineName)
        {
            if (alias == null) throw new ArgumentNullException(nameof(alias));

            // the alias may be "foo/bar" or "/foo/bar"
            // there may be spaces as in "/foo/bar,  /foo/nil"
            // these should probably be taken care of earlier on

            // TODO: can we normalize the values so that they contain no whitespaces, and no leading slashes?
            // and then the comparisons in IsMatch can be way faster - and allocate way less strings

            const string propertyAlias = Constants.Conventions.Content.UrlAlias;

            var hasDomain = !domaineName.IsNullOrWhiteSpace();

            // Try to find any first relative path
            var match = Regex.Match(domaineName, @"(?<!http:\/|http:|https:\/|https:)\/.+");

            // Create the list of potential alias that can be found on umbracoAliasUrl
            var trimAlias = hasDomain ? alias.TrimStart(match.Value) : alias;
            var testList = new List<string>
            {
                trimAlias.TrimStart('/'), // is "alias"
                trimAlias, // is "/alias"
                $"{trimAlias.TrimStart('/')}/", // is "alias/"
                trimAlias.EnsureEndsWith('/') // is "/alias/"
            };

            bool IsMatch(IPublishedContent c)
            {
                // this basically implements the original XPath query ;-(
                //
                // "//* [@isDoc and (" +
                // "contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',{0},')" +
                // " or contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',/{0},')" +
                // ")]"

                if (!c.HasProperty(propertyAlias)) return false;
                var p = c.GetProperty(propertyAlias);
                var varies = p.PropertyType.VariesByCulture();
                string v;
                if (varies)
                {
                    if (!c.HasCulture(culture)) return false;
                    v = c.Value<string>(propertyAlias, culture);
                }
                else
                {
                    v = c.Value<string>(propertyAlias);
                }
                if (string.IsNullOrWhiteSpace(v)) return false;
                v = v.Replace(" ", "");
                // Split UrlAlias to a list
                var t = v.Split(',');
                return t.ContainsAny(testList);
            }

            // TODO: even with Linq, what happens below has to be horribly slow
            // but the only solution is to entirely refactor url providers to stop being dynamic

            if (rootNodeId > 0)
            {
                var rootNode = cache.GetById(rootNodeId);
                return rootNode?.Descendants().FirstOrDefault(IsMatch);
            }

            foreach (var rootContent in cache.GetAtRoot())
            {
                var c = rootContent.DescendantsOrSelf().FirstOrDefault(IsMatch);
                if (c != null) return c;
            }

            return null;
        }
    }
}
