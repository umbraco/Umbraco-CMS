using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides URLs using the <c>umbracoUrlAlias</c> property.
    /// </summary>
    public class AliasUrlProvider : IUrlProvider
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IRequestHandlerSection _requestConfig;
        private readonly ISiteDomainHelper _siteDomainHelper;

        public AliasUrlProvider(IGlobalSettings globalSettings, IRequestHandlerSection requestConfig, ISiteDomainHelper siteDomainHelper)
        {
            _globalSettings = globalSettings;
            _requestConfig = requestConfig;
            _siteDomainHelper = siteDomainHelper;
        }

        // note - at the moment we seem to accept pretty much anything as an alias
        // without any form of validation ... could even prob. kill the XPath ...
        // ok, this is somewhat experimental and is NOT enabled by default

        #region GetUrl

        /// <inheritdoc />
        public UrlInfo GetUrl(UmbracoContext umbracoContext, IPublishedContent content, UrlMode mode, string culture, Uri current)
        {
            return null; // we have nothing to say
        }

        #endregion

        #region GetOtherUrls

        /// <summary>
        /// Gets the other URLs of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute URL.</param>
        /// <returns>The other URLs for the published content.</returns>
        /// <remarks>
        /// <para>Other URLs are those that <c>GetUrl</c> would not return in the current context, but would be valid
        /// URLs for the node in other contexts (different domain for current request, umbracoUrlAlias...).</para>
        /// </remarks>
        public IEnumerable<UrlInfo> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current)
        {
            var node = umbracoContext.Content.GetById(id);
            if (node == null)
                yield break;

            if (!node.HasProperty(Constants.Conventions.Content.UrlAlias))
                yield break;

            // look for domains, walking up the tree
            var n = node;
            var domainUris = DomainUtilities.DomainsForNode(umbracoContext.PublishedSnapshot.Domains, _siteDomainHelper, n.Id, current, false);
            while (domainUris == null && n != null) // n is null at root
            {
                // move to parent node
                n = n.Parent;
                domainUris = n == null ? null : DomainUtilities.DomainsForNode(umbracoContext.PublishedSnapshot.Domains, _siteDomainHelper, n.Id, current, excludeDefault: false);
            }

            // determine whether the alias property varies
            var varies = node.GetProperty(Constants.Conventions.Content.UrlAlias).PropertyType.VariesByCulture();

            if (domainUris == null)
            {
                // no domain
                // if the property is invariant, then URL "/<alias>" is ok
                // if the property varies, then what are we supposed to do?
                //  the content finder may work, depending on the 'current' culture,
                //  but there's no way we can return something meaningful here
                if (varies)
                    yield break;

                var umbracoUrlName = node.Value<string>(Constants.Conventions.Content.UrlAlias);
                var aliases = umbracoUrlName?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);

                if (aliases == null || aliases.Any() == false)
                    yield break;

                foreach (var alias in aliases.Distinct())
                {
                    var path = "/" + alias;
                    var uri = new Uri(path, UriKind.Relative);
                    yield return UrlInfo.Url(UriUtility.UriFromUmbraco(uri, _globalSettings, _requestConfig).ToString());
                }
            }
            else
            {
                // some domains: one URL per domain, which is "<domain>/<alias>"
                foreach (var domainUri in domainUris)
                {
                    // if the property is invariant, get the invariant value, URL is "<domain>/<invariant-alias>"
                    // if the property varies, get the variant value, URL is "<domain>/<variant-alias>"

                    // but! only if the culture is published, else ignore
                    if (varies && !node.HasCulture(domainUri.Culture.Name)) continue;

                    var umbracoUrlName = varies
                        ? node.Value<string>(Constants.Conventions.Content.UrlAlias, culture: domainUri.Culture.Name)
                        : node.Value<string>(Constants.Conventions.Content.UrlAlias);

                    var aliases = umbracoUrlName?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);

                    if (aliases == null || aliases.Any() == false)
                        continue;

                    foreach(var alias in aliases.Distinct())
                    {
                        var path = "/" + alias;
                        var uri = new Uri(CombinePaths(domainUri.Uri.GetLeftPart(UriPartial.Path), path));
                        yield return UrlInfo.Url(UriUtility.UriFromUmbraco(uri, _globalSettings, _requestConfig).ToString(), domainUri.Culture.Name);
                    }
                }
            }
        }

        #endregion

        #region Utilities

        string CombinePaths(string path1, string path2)
        {
            string path = path1.TrimEnd(Constants.CharArrays.ForwardSlash) + path2;
            return path == "/" ? path : path.TrimEnd(Constants.CharArrays.ForwardSlash);
        }

        #endregion
    }
}
