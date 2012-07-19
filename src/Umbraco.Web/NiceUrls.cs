using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.IO;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web
{
    internal class NiceUrls
    {
        public NiceUrls(ContentStore contentStore, UmbracoContext umbracoContext, RoutesCache routesCache)
        {
            _umbracoContext = umbracoContext;
            _contentStore = contentStore;
            _routesCache = routesCache;
        }

        private readonly UmbracoContext _umbracoContext;
        private readonly ContentStore _contentStore;
        private readonly RoutesCache _routesCache;

        // note: this could be a parameter...
        const string UrlNameProperty = "@urlName";

        public virtual string GetNiceUrl(int nodeId)
        {
            int startNodeDepth = 1;
            if (GlobalSettings.HideTopLevelNodeFromPath)
                startNodeDepth = 2;

            return GetNiceUrl(nodeId, startNodeDepth, false);
        }

        public virtual string GetNiceUrl(int nodeId, int startNodeDepth, bool forceDomain)
        {
            string route;
            string path;

            route = _routesCache.GetRoute(nodeId); // will not read cache if previewing
            if (route != null)
            {
                int pos = route.IndexOf('/');
                path = route.Substring(pos);

                if (UmbracoSettings.UseDomainPrefixes || forceDomain)
                {
                    int rootNodeId = int.Parse(route.Substring(0, pos));
                    if (rootNodeId > 0)
                        return DomainAtNode(rootNodeId) + path;
                }

                return path;
            }

            // else there was not route in the cache, must build route...

            var node = _contentStore.GetNodeById(nodeId);
            if (node == null)
                return "#"; // legacy wrote to the log here...

            var parts = new List<string>();
            var depth = int.Parse(_contentStore.GetNodeProperty(node, "@level"));
            var id = nodeId;
            string domain = null;
            while (depth >= 1)
            {
                // if not hiding that depth, add urlName
                if (depth >= startNodeDepth)
                    parts.Add(_contentStore.GetNodeProperty(node, UrlNameProperty));

                var tmp = DomainAtNode(id);
                if (tmp != null)
                {
                    if (UmbracoSettings.UseDomainPrefixes || forceDomain)
                        domain = tmp;
                    break; // break to capture the id
                }

                node = _contentStore.GetNodeParent(node);
                id = int.Parse(_contentStore.GetNodeProperty(node, "@id"));
                depth--;
            }

            parts.Reverse();
            path = "/" + string.Join("/", parts);
            route = string.Format("{0}{1}", id, path);
            _routesCache.Store(nodeId, route); // will not write if previewing

            return FormatUrl(domain, path);
        }

        protected string DomainAtNode(int nodeId)
        {
            // be safe
            if (nodeId <= 0)
                return null;

            // get domains defined on that node
            Domain[] domains = Domain.GetDomainsById(nodeId);

            // no domain set on that node, return null
            if (domains.Length == 0)
                return null;

            // else try to find the first domain that matches the current request
            // else take the first domain of the list
            Domain domain = domains.FirstOrDefault(d => UrlUtility.IsBaseOf(d.Name, _umbracoContext.OriginalUrl)) ?? domains[0];

            var domainName = domain.Name.TrimEnd('/');
            domainName = UrlUtility.EnsureScheme(domainName, _umbracoContext.OriginalUrl.Scheme);
            var pos = domainName.IndexOf("//");
            pos = domainName.IndexOf("/", pos + 2);
            if (pos > 0)
                domainName = domainName.Substring(0, pos);

            // return a scheme + host eg http://example.com with no trailing slash
            return domainName;
        }

        protected string FormatUrl(string domain, string path)
        {
            if (domain == null) // else vdir needs to be in the domain
            {
                // get the application virtual dir (empty if no vdir)
                string vdir = SystemDirectories.Root;
                if (!string.IsNullOrEmpty(vdir))
                    domain = "/" + vdir;
            }

            string url = (domain ?? "") + path;
            if (path != "/")
            {
                // not at root
                if (GlobalSettings.UseDirectoryUrls)
                {
                    // add trailing / if required
                    if (UmbracoSettings.AddTrailingSlash)
                        url += "/";
                }
                else
                {
                    // add .aspx
                    url += ".aspx";
                }
            }

            return url;
        }
    }
}