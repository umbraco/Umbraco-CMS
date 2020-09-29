using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PublishedCache
{
    internal class ContentCacheContentRouter : IContentRouter
    {
        private readonly IGlobalSettings _globalSettings;

        public ContentCacheContentRouter(IGlobalSettings globalSettings)
        {
            _globalSettings = globalSettings;
        }

        public ContentRoutingResult GetIdByRoute(IPublishedCache2 contentCache,bool defaultPreview,string route, bool? hideTopLevelNode = null, string culture = null)
        {
            return GetIdByRoute(contentCache, defaultPreview, route, hideTopLevelNode, culture);
        }

        public ContentRoutingResult GetIdByRoute(IPublishedSnapshot snapshot, bool preview, string route, bool? hideTopLevelNode = null, string culture = null)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            return GetByRouteInternal(snapshot,preview, route, hideTopLevelNode, culture);
        }


        internal ContentRoutingResult GetByRouteInternal(IPublishedSnapshot snapshot, bool preview, string route, bool? hideTopLevelNode, string culture)
        {
            hideTopLevelNode = hideTopLevelNode ?? _globalSettings.HideTopLevelNodeFromPath; // default = settings

            // the route always needs to be lower case because we only store the urlName attribute in lower case
            route = route.ToLowerInvariant();

            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);
            var startNodeId = pos == 0 ? 0 : int.Parse(route.Substring(0, pos));
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            IPublishedContent content;

            if (startNodeId > 0)
            {
                // if in a domain then start with the root node of the domain
                // and follow the path
                // note: if domain has a path (eg example.com/en) which is not recommended anymore
                //  then /en part of the domain is basically ignored here...
                content = snapshot.Content.GetById(preview, startNodeId);
                content = FollowRoute(content, parts, 0, culture);
            }
            else if (parts.Length == 0)
            {
                // if not in a domain, and path is empty - what is the default page?
                // let's say it is the first one in the tree, if any -- order by sortOrder
                content = snapshot.Content.GetAtRoot(preview).FirstOrDefault();
            }
            else
            {
                // if not in a domain...
                // hideTopLevelNode = support legacy stuff, look for /*/path/to/node
                // else normal, look for /path/to/node
                content = hideTopLevelNode.Value
                    ? snapshot.Content.GetAtRoot(preview).SelectMany(x => x.Children(culture)).FirstOrDefault(x => x.UrlSegment(culture) == parts[0])
                    : snapshot.Content.GetAtRoot(preview).FirstOrDefault(x => x.UrlSegment(culture) == parts[0]);
                content = FollowRoute(content, parts, 1, culture);
            }

            // if hideTopLevelNodePath is true then for url /foo we looked for /*/foo
            // but maybe that was the url of a non-default top-level node, so we also
            // have to look for /foo (see note in ApplyHideTopLevelNodeFromPath).
            if (content == null && hideTopLevelNode.Value && parts.Length == 1)
            {
                content = snapshot.Content.GetAtRoot(preview).FirstOrDefault(x => x.UrlSegment(culture) == parts[0]);
            }
            ContentRoutingResult result = new ContentRoutingResult
            (
                content != null ? RoutingOutcome.Found : RoutingOutcome.NotFound,
                content?.Id ?? 0
            );
            return result;
        }
        private IPublishedContent FollowRoute(IPublishedContent content, IReadOnlyList<string> parts, int start, string culture)
        {
            var i = start;
            while (content != null && i < parts.Count)
            {
                var part = parts[i++];
                content = content.Children(culture).FirstOrDefault(x =>
                {
                    var urlSegment = x.UrlSegment(culture);
                    return urlSegment == part;
                });
            }
            return content;
        }

        public string GetRouteById(bool defaultPreview, IPublishedCache2 publishedCache, IDomainCache domainCache, int contentId, string culture = null)
        {
            return GetRouteById(publishedCache, domainCache,defaultPreview, contentId, culture);
        }

        public string GetRouteById(IPublishedCache2 snapshot, IDomainCache domainCache, bool preview, int contentId, string culture = null)
        {
            return GetRouteByIdInternal(snapshot, domainCache,preview, contentId, null, culture);
        }

        private string GetRouteByIdInternal(IPublishedCache2 publishedCache, IDomainCache domainCache, bool preview, int contentId, bool? hideTopLevelNode, string culture)
        {
            var node = publishedCache.GetById(preview, contentId);
            if (node == null)
                return null;

            hideTopLevelNode = hideTopLevelNode ?? _globalSettings.HideTopLevelNodeFromPath; // default = settings

            // walk up from that node until we hit a node with a domain,
            // or we reach the content root, collecting urls in the way
            var pathParts = new List<string>();
            var n = node;
            var urlSegment = n.UrlSegment(culture);
            var hasDomains = domainCache.HasAssigned(n.Id);
            while (hasDomains == false && n != null) // n is null at root
            {
                // no segment indicates this is not published when this is a variant
                if (urlSegment.IsNullOrWhiteSpace()) return null;

                pathParts.Add(urlSegment);

                // move to parent node
                n = n.Parent;
                if (n != null)
                    urlSegment = n.UrlSegment(culture);

                hasDomains = n != null && domainCache.HasAssigned(n.Id);
            }

            // at this point this will be the urlSegment of the root, no segment indicates this is not published when this is a variant
            if (urlSegment.IsNullOrWhiteSpace()) return null;

            // no domain, respect HideTopLevelNodeFromPath for legacy purposes
            if (hasDomains == false && hideTopLevelNode.Value)
                ApplyHideTopLevelNodeFromPath(publishedCache,node, pathParts, preview);

            // assemble the route
            pathParts.Reverse();
            var path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
            //prefix the root node id containing the domain if it exists (this is a standard way of creating route paths)
            //and is done so that we know the ID of the domain node for the path
            var route = (n?.Id.ToString(CultureInfo.InvariantCulture) ?? "") + path;

            return route;
        }

        private void ApplyHideTopLevelNodeFromPath(IPublishedCache2 publishedCache, IPublishedContent content, IList<string> segments, bool preview)
        {
            // in theory if hideTopLevelNodeFromPath is true, then there should be only one
            // top-level node, or else domains should be assigned. but for backward compatibility
            // we add this check - we look for the document matching "/" and if it's not us, then
            // we do not hide the top level path
            // it has to be taken care of in GetByRoute too so if
            // "/foo" fails (looking for "/*/foo") we try also "/foo".
            // this does not make much sense anyway esp. if both "/foo/" and "/bar/foo" exist, but
            // that's the way it works pre-4.10 and we try to be backward compat for the time being
            if (content.Parent == null)
            {
                var rootNode = publishedCache.GetById(preview, GetIdByRoute(publishedCache,preview, "/", true).Id);
                if (rootNode == null)
                    throw new Exception("Failed to get node at /.");
                if (rootNode.Id == content.Id) // remove only if we're the default node
                    segments.RemoveAt(segments.Count - 1);
            }
            else
            {
                segments.RemoveAt(segments.Count - 1);
            }
        }

        public ContentRoutingResult GetIdByAlias(IPublishedContentCache snapshot, bool preview, int rootNodeId, string culture, string alias)
        {
            var content = FindContentByAlias(snapshot, rootNodeId, culture, alias,preview);
            return new ContentRoutingResult
            (

                content != null ? RoutingOutcome.Found : RoutingOutcome.NotFound,
                 content?.Id ?? 0
            );
        }
        private static IPublishedContent FindContentByAlias(IPublishedCache2 publishedCache, int rootNodeId, string culture, string alias,bool preview)
        {
            if (alias == null) throw new ArgumentNullException(nameof(alias));

            // the alias may be "foo/bar" or "/foo/bar"
            // there may be spaces as in "/foo/bar,  /foo/nil"
            // these should probably be taken care of earlier on

            // TODO: can we normalize the values so that they contain no whitespaces, and no leading slashes?
            // and then the comparisons in IsMatch can be way faster - and allocate way less strings

            const string propertyAlias = Constants.Conventions.Content.UrlAlias;

            var test1 = alias.TrimStart('/') + ",";
            var test2 = ",/" + test1; // test2 is ",/alias,"
            test1 = "," + test1; // test1 is ",alias,"

            bool IsMatch(IPublishedContent c, string a1, string a2)
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
                v = "," + v.Replace(" ", "") + ",";
                return v.InvariantContains(a1) || v.InvariantContains(a2);
            }

            // TODO: even with Linq, what happens below has to be horribly slow
            // but the only solution is to entirely refactor url providers to stop being dynamic

            if (rootNodeId > 0)
            {
                var rootNode = publishedCache.GetById(preview,rootNodeId);
                return rootNode?.Descendants().FirstOrDefault(x => IsMatch(x, test1, test2));
            }

            foreach (var rootContent in publishedCache.GetAtRoot())
            {
                var c = rootContent.DescendantsOrSelf().FirstOrDefault(x => IsMatch(x, test1, test2));
                if (c != null) return c;
            }

            return null;
        }
        private static IPublishedContent FindContentByAlias(IPublishedContentCache publishedCache, int rootNodeId, string culture, string alias, bool preview)
        {
            if (alias == null) throw new ArgumentNullException(nameof(alias));

            // the alias may be "foo/bar" or "/foo/bar"
            // there may be spaces as in "/foo/bar,  /foo/nil"
            // these should probably be taken care of earlier on

            // TODO: can we normalize the values so that they contain no whitespaces, and no leading slashes?
            // and then the comparisons in IsMatch can be way faster - and allocate way less strings

            const string propertyAlias = Constants.Conventions.Content.UrlAlias;

            var test1 = alias.TrimStart('/') + ",";
            var test2 = ",/" + test1; // test2 is ",/alias,"
            test1 = "," + test1; // test1 is ",alias,"

            bool IsMatch(IPublishedContent c, string a1, string a2)
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
                v = "," + v.Replace(" ", "") + ",";
                return v.InvariantContains(a1) || v.InvariantContains(a2);
            }

            // TODO: even with Linq, what happens below has to be horribly slow
            // but the only solution is to entirely refactor url providers to stop being dynamic

            if (rootNodeId > 0)
            {
                var rootNode = publishedCache.GetById(preview, rootNodeId);
                return rootNode?.Descendants().FirstOrDefault(x => IsMatch(x, test1, test2));
            }

            foreach (var rootContent in publishedCache.GetAtRoot())
            {
                var c = rootContent.DescendantsOrSelf().FirstOrDefault(x => IsMatch(x, test1, test2));
                if (c != null) return c;
            }

            return null;
        }


    }
}
