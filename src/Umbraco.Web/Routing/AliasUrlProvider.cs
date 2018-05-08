﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls using the <c>umbracoUrlAlias</c> property.
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
        public string GetUrl(UmbracoContext umbracoContext, IPublishedContent content, UrlProviderMode mode, string culture, Uri current)
        {
            return null; // we have nothing to say
        }

        #endregion

        #region GetOtherUrls

        /// <summary>
        /// Gets the other urls of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The other urls for the published content.</returns>
        /// <remarks>
        /// <para>Other urls are those that <c>GetUrl</c> would not return in the current context, but would be valid
        /// urls for the node in other contexts (different domain for current request, umbracoUrlAlias...).</para>
        /// </remarks>
        public IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current)
        {
            var node = umbracoContext.ContentCache.GetById(id);
            if (node == null)
                return Enumerable.Empty<string>();

            if (!node.HasProperty(Constants.Conventions.Content.UrlAlias))
                return Enumerable.Empty<string>();

            var domainHelper = umbracoContext.GetDomainHelper(_siteDomainHelper);

            var n = node;
            var domainUris = domainHelper.DomainsForNode(n.Id, current, false);
            while (domainUris == null && n != null) // n is null at root
            {
                // move to parent node
                n = n.Parent;
                domainUris = n == null ? null : domainHelper.DomainsForNode(n.Id, current, false);
            }

            if (domainUris == null)
            {
                var umbracoUrlName = node.Value<string>(Constants.Conventions.Content.UrlAlias);
                if (string.IsNullOrWhiteSpace(umbracoUrlName))
                    return Enumerable.Empty<string>();

                var path = "/" + umbracoUrlName;
                var uri = new Uri(path, UriKind.Relative);
                return new[] { UriUtility.UriFromUmbraco(uri, _globalSettings, _requestConfig).ToString() };
            }
            else
            {
                var result = new List<string>();
                foreach(var domainUri in domainUris)
                {
                    var umbracoUrlName = node.Value<string>(Constants.Conventions.Content.UrlAlias, culture: domainUri.Culture.Name);
                    if (!string.IsNullOrWhiteSpace(umbracoUrlName))
                    {
                        var path = "/" + umbracoUrlName;
                        var uri = new Uri(CombinePaths(domainUri.Uri.GetLeftPart(UriPartial.Path), path));
                        result.Add(UriUtility.UriFromUmbraco(uri, _globalSettings, _requestConfig).ToString());
                    }
                }
                return result;
            }
        }

        #endregion

        #region Utilities

        string CombinePaths(string path1, string path2)
        {
            string path = path1.TrimEnd('/') + path2;
            return path == "/" ? path : path.TrimEnd('/');
        }

        #endregion
    }
}
