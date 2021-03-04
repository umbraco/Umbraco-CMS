using System;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
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
                    frequest.Uri.AbsoluteUri,
                    frequest.UmbracoContext);

                if (node != null)
                {
                    frequest.PublishedContent = node;
                    Logger.Debug<ContentFinderByUrlAlias>("Path '{UriAbsolutePath}' is an alias for id={PublishedContentId}", frequest.Uri.AbsolutePath, frequest.PublishedContent.Id);
                }
            }

            return node != null;
        }

        private static IPublishedContent FindContentByAlias(IPublishedContentCache cache, int rootNodeId, string culture, string absolutePath, string absoluteUri, UmbracoContext umbracoContext)
        {
            if (absolutePath == null) throw new ArgumentNullException(nameof(absolutePath));

            // the alias may be "foo/bar" or "/foo/bar"
            // there may be spaces as in "/foo/bar,  /foo/nil"
            // these should probably be taken care of earlier on

            // TODO: can we normalize the values so that they contain no whitespaces, and no leading slashes?
            // and then the comparisons in IsMatch can be way faster - and allocate way less strings

            const string propertyAlias = Constants.Conventions.Content.UrlAlias;

            var test1 = absolutePath.TrimStart(Constants.CharArrays.ForwardSlash) + ",";
            var test2 = ",/" + test1; // test2 is ",/alias,"
            test1 = "," + test1; // test1 is ",alias,"

            bool IsMatch(IPublishedContent c, string requestAlias1, string requestAlias2, string requestAbsoluteUrl, string rootAbsoluteUrl)
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

                // Build up a comparison list of the current node's urlAlias to compare with the current request
                var sb = new StringBuilder(",");
                foreach (var urlAlias in v.Split(','))
                {
                    // Add the node's urlAlias to the list
                    sb.Append(urlAlias.Replace(" ", "") + ",");
                    // Add the root node's absolute url joined to the node's urlAlias to the list
                    sb.Append(rootAbsoluteUrl + (urlAlias.StartsWith("/") || rootAbsoluteUrl.EndsWith("/") ? "" : "/") + urlAlias.Replace(" ", "") + ",");
                }
                var urlAliases = sb.ToString();
                return urlAliases.InvariantContains(requestAlias1) || urlAliases.InvariantContains(requestAlias2) || urlAliases.InvariantContains(requestAbsoluteUrl);
            }

            // TODO: even with Linq, what happens below has to be horribly slow
            // but the only solution is to entirely refactor URL providers to stop being dynamic

            if (rootNodeId > 0)
            {
                var rootNode = cache.GetById(rootNodeId);
                var rootNodeAbsoluteUrl = umbracoContext?.Url(rootNodeId, UrlMode.Absolute, culture);
                return rootNode?.DescendantsOrSelf().FirstOrDefault(x => IsMatch(x, test1, test2, absoluteUri, rootNodeAbsoluteUrl));
            }

            foreach (var rootContent in cache.GetAtRoot())
            {
                var rootNodeAbsoluteUrl = umbracoContext?.Url(rootNodeId, UrlMode.Absolute, culture);
                var c = rootContent.DescendantsOrSelf().FirstOrDefault(x => IsMatch(x, test1, test2, absoluteUri, rootNodeAbsoluteUrl));
                if (c != null) return c;
            }

            return null;
        }
    }
}
