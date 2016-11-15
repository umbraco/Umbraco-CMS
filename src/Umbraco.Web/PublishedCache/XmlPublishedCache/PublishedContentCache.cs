using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Web.Routing;
using umbraco;
using System.Linq;
using umbraco.BusinessLogic;
using umbraco.presentation.preview;
using Umbraco.Core.Services;
using GlobalSettings = umbraco.GlobalSettings;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    internal class PublishedContentCache : IPublishedContentCache
    {
        #region Routes cache

        private readonly RoutesCache _routesCache = new RoutesCache(!UnitTesting);

        private DomainHelper _domainHelper;

        private DomainHelper GetDomainHelper(IDomainService domainService)
        {
            return _domainHelper ?? (_domainHelper = new DomainHelper(domainService));
        }

        // for INTERNAL, UNIT TESTS use ONLY
        internal RoutesCache RoutesCache { get { return _routesCache; } }

        // for INTERNAL, UNIT TESTS use ONLY
        internal static bool UnitTesting = false;

        public virtual IPublishedContent GetByRoute(UmbracoContext umbracoContext, bool preview, string route, bool? hideTopLevelNode = null)
        {
            if (route == null) throw new ArgumentNullException("route");

            // try to get from cache if not previewing
            var contentId = preview ? 0 : _routesCache.GetNodeId(route);

            // if found id in cache then get corresponding content
            // and clear cache if not found - for whatever reason
            IPublishedContent content = null;
            if (contentId > 0)
            {
                content = GetById(umbracoContext, preview, contentId);
                if (content == null)
                    _routesCache.ClearNode(contentId);
            }

            // still have nothing? actually determine the id
            hideTopLevelNode = hideTopLevelNode ?? GlobalSettings.HideTopLevelNodeFromPath; // default = settings
            content = content ?? DetermineIdByRoute(umbracoContext, preview, route, hideTopLevelNode.Value);

            // cache if we have a content and not previewing
            if (content != null && preview == false)
                AddToCacheIfDeepestRoute(umbracoContext, content, route);

            return content;
        }

        private void AddToCacheIfDeepestRoute(UmbracoContext umbracoContext, IPublishedContent content, string route)
        {
            var domainRootNodeId = route.StartsWith("/") ? -1 : int.Parse(route.Substring(0, route.IndexOf('/')));

            // so we have a route that maps to a content... say "1234/path/to/content" - however, there could be a
            // domain set on "to" and route "4567/content" would also map to the same content - and due to how
            // urls computing work (by walking the tree up to the first domain we find) it is that second route
            // that would be returned - the "deepest" route - and that is the route we want to cache, *not* the
            // longer one - so make sure we don't cache the wrong route

            var deepest = UnitTesting == false
                && DomainHelper.ExistsDomainInPath(umbracoContext.Application.Services.DomainService.GetAll(false), content.Path, domainRootNodeId) == false;

            if (deepest)
                _routesCache.Store(content.Id, route);
        }

        public virtual string GetRouteById(UmbracoContext umbracoContext, bool preview, int contentId)
        {
            // try to get from cache if not previewing
            var route = preview ? null : _routesCache.GetRoute(contentId);

            // if found in cache then return
            if (route != null)
                return route;

            // else actually determine the route
            route = DetermineRouteById(umbracoContext, preview, contentId);

            // node not found
            if (route == null)
                return null;

            // find the content back, detect routes collisions: we should find ourselves back,
            // else it means that another content with "higher priority" is sharing the same route.
            // perf impact:
            // - non-colliding, adds one complete "by route" lookup, only on the first time a url is computed (then it's cached anyways)
            // - colliding, adds one "by route" lookup, the first time the url is computed, then one dictionary looked each time it is computed again
            // assuming no collisions, the impact is one complete "by route" lookup the first time each url is computed
            //
            // U4-9121 - this lookup is too expensive when computing a large amount of urls on a front-end (eg menu)
            // ... thinking about moving the lookup out of the path into its own async task, so we are not reporting errors
            //     in the back-office anymore, but at least we are not polluting the cache
            // instead, refactored DeterminedIdByRoute to stop using XPath, with a 16x improvement according to benchmarks
            // will it be enough?

            var loopId = preview ? 0 : _routesCache.GetNodeId(route); // might be cached already in case of collision
            if (loopId == 0)
            {
                var content = DetermineIdByRoute(umbracoContext, preview, route, GlobalSettings.HideTopLevelNodeFromPath);

                // add the other route to cache so next time we have it already
                if (content != null && preview == false)
                    AddToCacheIfDeepestRoute(umbracoContext, content, route);

                loopId = content == null ? 0 : content.Id; // though... 0 here would be quite weird?
            }

            // cache if we have a route and not previewing and it's not a colliding route
            // (the result of DetermineRouteById is always the deepest route)
            if (/*route != null &&*/ preview == false && loopId == contentId)
                _routesCache.Store(contentId, route);

            // return route if no collision, else report collision
            return loopId == contentId ? route : ("err/" + loopId);
        }

        IPublishedContent DetermineIdByRoute(UmbracoContext umbracoContext, bool preview, string route, bool hideTopLevelNode)
        {
            if (route == null) throw new ArgumentNullException("route");

            //the route always needs to be lower case because we only store the urlName attribute in lower case
            route = route.ToLowerInvariant();

            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);
            var startNodeId = pos == 0 ? 0 : int.Parse(route.Substring(0, pos));

            //check if we can find the node in our xml cache
            var id = NavigateRoute(umbracoContext, preview, startNodeId, path, hideTopLevelNode);
            if (id > 0) return GetById(umbracoContext, preview, id);

            // if hideTopLevelNodePath is true then for url /foo we looked for /*/foo
            // but maybe that was the url of a non-default top-level node, so we also
            // have to look for /foo (see note in ApplyHideTopLevelNodeFromPath).
            if (hideTopLevelNode && path.Length > 1 && path.IndexOf('/', 1) < 0)
            {
                var id2 = NavigateRoute(umbracoContext, preview, startNodeId, path, false);
                if (id2 > 0) return GetById(umbracoContext, preview, id2);
            }

            return null;
        }

        private int NavigateRoute(UmbracoContext umbracoContext, bool preview, int startNodeId, string path, bool hideTopLevelNode)
        {
            var xml = GetXml(umbracoContext, preview);
            XmlElement elt;

            // empty path
            if (path == string.Empty || path == "/")
            {
                if (startNodeId > 0)
                {
                    elt = xml.GetElementById(startNodeId.ToString(CultureInfo.InvariantCulture));
                    return elt == null ? -1 : startNodeId;
                }

                elt = null;
                var min = int.MaxValue;
                foreach (XmlElement e in xml.DocumentElement.ChildNodes)
                {
                    var sortOrder = int.Parse(e.GetAttribute("sortOrder"));
                    if (sortOrder < min)
                    {
                        min = sortOrder;
                        elt = e;
                    }
                }
                return elt == null ? -1 : int.Parse(elt.GetAttribute("id"));
            }

            // non-empty path
            elt = startNodeId <= 0
                ? xml.DocumentElement
                : xml.GetElementById(startNodeId.ToString(CultureInfo.InvariantCulture));
            if (elt == null) return -1;

            var urlParts = path.Split(SlashChar, StringSplitOptions.RemoveEmptyEntries);

            if (hideTopLevelNode && startNodeId <= 0)
            {
                foreach (XmlElement e in elt.ChildNodes)
                {
                    var id = NavigateElementRoute(e, urlParts);
                    if (id > 0) return id;
                }
                return -1;
            }

            return NavigateElementRoute(elt, urlParts);
        }

        private static bool UseLegacySchema
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema; }
        }

        private int NavigateElementRoute(XmlElement elt, string[] urlParts)
        {
            var found = true;
            var i = 0;
            while (found && i < urlParts.Length)
            {
                found = false;
                foreach (XmlElement child in elt.ChildNodes)
                {
                    var noNode = UseLegacySchema
                        ? child.Name != "node"
                        : child.GetAttributeNode("isDoc") == null;
                    if (noNode) continue;
                    if (child.GetAttribute("urlName") != urlParts[i]) continue;

                    found = true;
                    elt = child;
                    break;
                }
                i++;
            }
            return found ? int.Parse(elt.GetAttribute("id")) : -1;
        }

        string DetermineRouteById(UmbracoContext umbracoContext, bool preview, int contentId)
        {
            var elt = GetXml(umbracoContext, preview).GetElementById(contentId.ToString(CultureInfo.InvariantCulture));
            if (elt == null) return null;

            var domainHelper = GetDomainHelper(umbracoContext.Application.Services.DomainService);

            // walk up from that node until we hit a node with a domain,
            // or we reach the content root, collecting urls in the way
            var pathParts = new List<string>();
            var eltId = int.Parse(elt.GetAttribute("id"));
            var eltParentId = int.Parse(((XmlElement) elt.ParentNode).GetAttribute("id"));
            var e = elt;
            var id = eltId;
            var hasDomains = domainHelper.NodeHasDomains(id);
            while (hasDomains == false && id != -1)
            {
                // get the url
                var urlName = e.GetAttribute("urlName");
                pathParts.Add(urlName);

                // move to parent node
                e = (XmlElement) e.ParentNode;
                id = int.Parse(e.GetAttribute("id"));
                hasDomains = id != -1 && domainHelper.NodeHasDomains(id);
            }

            // no domain, respect HideTopLevelNodeFromPath for legacy purposes
            if (hasDomains == false && GlobalSettings.HideTopLevelNodeFromPath)
                ApplyHideTopLevelNodeFromPath(umbracoContext, eltId, eltParentId, pathParts);

            // assemble the route
            pathParts.Reverse();
            var path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
            var route = (id == -1 ? "" : id.ToString(CultureInfo.InvariantCulture)) + path;

            return route;
        }

        static void ApplyHideTopLevelNodeFromPath(UmbracoContext umbracoContext, int nodeId, int parentId, IList<string> pathParts)
        {
            // in theory if hideTopLevelNodeFromPath is true, then there should be only once
            // top-level node, or else domains should be assigned. but for backward compatibility
            // we add this check - we look for the document matching "/" and if it's not us, then
            // we do not hide the top level path
            // it has to be taken care of in GetByRoute too so if
            // "/foo" fails (looking for "/*/foo") we try also "/foo".
            // this does not make much sense anyway esp. if both "/foo/" and "/bar/foo" exist, but
            // that's the way it works pre-4.10 and we try to be backward compat for the time being
            if (parentId == -1)
            {
                var rootNode = umbracoContext.ContentCache.GetByRoute("/", true);
                if (rootNode == null)
                    throw new Exception("Failed to get node at /.");
                if (rootNode.Id == nodeId) // remove only if we're the default node
                    pathParts.RemoveAt(pathParts.Count - 1);
            }
            else
            {
                pathParts.RemoveAt(pathParts.Count - 1);
            }
        }

        #endregion

        #region XPath Strings

        class XPathStringsDefinition
		{
			public int Version { get; private set; }

			public string RootDocuments { get; private set; }

			public XPathStringsDefinition(int version)
			{
				Version = version;

				switch (version)
				{
					// legacy XML schema
					case 0:
						RootDocuments = "/root/node";
						break;

					// default XML schema as of 4.10
					case 1:
						RootDocuments = "/root/* [@isDoc]";
						break;

					default:
						throw new Exception(string.Format("Unsupported Xml schema version '{0}').", version));
				}
			}
		}

		static XPathStringsDefinition _xPathStringsValue;
		static XPathStringsDefinition XPathStrings
		{
			get
			{
				// in theory XPathStrings should be a static variable that
				// we should initialize in a static ctor - but then test cases
				// that switch schemas fail - so cache and refresh when needed,
				// ie never when running the actual site

				var version = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? 0 : 1;
				if (_xPathStringsValue == null || _xPathStringsValue.Version != version)
					_xPathStringsValue = new XPathStringsDefinition(version);
				return _xPathStringsValue;
			}
		}

		#endregion

        #region Converters

        private static IPublishedContent ConvertToDocument(XmlNode xmlNode, bool isPreviewing)
		{
		    return xmlNode == null ? null : XmlPublishedContent.Get(xmlNode, isPreviewing);
		}

        private static IEnumerable<IPublishedContent> ConvertToDocuments(XmlNodeList xmlNodes, bool isPreviewing)
        {
            return xmlNodes.Cast<XmlNode>()
                .Select(xmlNode => XmlPublishedContent.Get(xmlNode, isPreviewing));
        }

        #endregion

        #region Getters

        public virtual IPublishedContent GetById(UmbracoContext umbracoContext, bool preview, int nodeId)
    	{
    		return ConvertToDocument(GetXml(umbracoContext, preview).GetElementById(nodeId.ToString(CultureInfo.InvariantCulture)), preview);
    	}

        public virtual IEnumerable<IPublishedContent> GetAtRoot(UmbracoContext umbracoContext, bool preview)
        {
            return ConvertToDocuments(GetXml(umbracoContext, preview).SelectNodes(XPathStrings.RootDocuments), preview);
		}

        public virtual IPublishedContent GetSingleByXPath(UmbracoContext umbracoContext, bool preview, string xpath, params XPathVariable[] vars)
        {
            if (xpath == null) throw new ArgumentNullException("xpath");
            if (string.IsNullOrWhiteSpace(xpath)) return null;

            var xml = GetXml(umbracoContext, preview);
            var node = vars == null
                ? xml.SelectSingleNode(xpath)
                : xml.SelectSingleNode(xpath, vars);
            return ConvertToDocument(node, preview);
        }

        public virtual IPublishedContent GetSingleByXPath(UmbracoContext umbracoContext, bool preview, XPathExpression xpath, params XPathVariable[] vars)
        {
            if (xpath == null) throw new ArgumentNullException("xpath");

            var xml = GetXml(umbracoContext, preview);
            var node = vars == null
                ? xml.SelectSingleNode(xpath)
                : xml.SelectSingleNode(xpath, vars);
            return ConvertToDocument(node, preview);
        }

        public virtual IEnumerable<IPublishedContent> GetByXPath(UmbracoContext umbracoContext, bool preview, string xpath, params XPathVariable[] vars)
        {
            if (xpath == null) throw new ArgumentNullException("xpath");
            if (string.IsNullOrWhiteSpace(xpath)) return Enumerable.Empty<IPublishedContent>();

            var xml = GetXml(umbracoContext, preview);
            var nodes = vars == null
                ? xml.SelectNodes(xpath)
                : xml.SelectNodes(xpath, vars);
            return ConvertToDocuments(nodes, preview);
        }

        public virtual IEnumerable<IPublishedContent> GetByXPath(UmbracoContext umbracoContext, bool preview, XPathExpression xpath, params XPathVariable[] vars)
        {
            if (xpath == null) throw new ArgumentNullException("xpath");

            var xml = GetXml(umbracoContext, preview);
            var nodes = vars == null
                ? xml.SelectNodes(xpath)
                : xml.SelectNodes(xpath, vars);
            return ConvertToDocuments(nodes, preview);
        }

        public virtual bool HasContent(UmbracoContext umbracoContext, bool preview)
        {
	        var xml = GetXml(umbracoContext, preview);
			if (xml == null)
				return false;
			var node = xml.SelectSingleNode(XPathStrings.RootDocuments);
			return node != null;
        }

        public virtual XPathNavigator GetXPathNavigator(UmbracoContext umbracoContext, bool preview)
        {
            var xml = GetXml(umbracoContext, preview);
            return xml.CreateNavigator();
        }

        public virtual bool XPathNavigatorIsNavigable { get { return false; } }

        #endregion

        #region Legacy Xml

        static readonly ConditionalWeakTable<UmbracoContext, PreviewContent> PreviewContentCache
            = new ConditionalWeakTable<UmbracoContext, PreviewContent>();

        private Func<UmbracoContext, bool, XmlDocument> _xmlDelegate;

        /// <summary>
        /// Gets/sets the delegate used to retrieve the Xml content, generally the setter is only used for unit tests
        /// and by default if it is not set will use the standard delegate which ONLY works when in the context an Http Request
        /// </summary>
        /// <remarks>
        /// If not defined, we will use the standard delegate which ONLY works when in the context an Http Request
        /// mostly because the 'content' object heavily relies on HttpContext, SQL connections and a bunch of other stuff
        /// that when run inside of a unit test fails.
        /// </remarks>
        internal Func<UmbracoContext, bool, XmlDocument> GetXmlDelegate
        {
            get
            {
                return _xmlDelegate ?? (_xmlDelegate = (context, preview) =>
                {
                    if (preview)
                    {
                        var previewContent = PreviewContentCache.GetOrCreateValue(context); // will use the ctor with no parameters
                        previewContent.EnsureInitialized(context.UmbracoUser, StateHelper.Cookies.Preview.GetValue(), true, () =>
                        {
                            if (previewContent.ValidPreviewSet)
                                previewContent.LoadPreviewset();
                        });
                        if (previewContent.ValidPreviewSet)
                            return previewContent.XmlContent;
                    }
                    return content.Instance.XmlContent;
                });
            }
            set
            {
                _xmlDelegate = value;
            }
        }

        internal XmlDocument GetXml(UmbracoContext umbracoContext, bool preview)
        {
            var xml = GetXmlDelegate(umbracoContext, preview);
            if (xml == null)
                throw new Exception("The Xml cache is corrupt. Use the Health Check data integrity dashboard to fix it.");
            return xml;
        }

        #endregion

        #region XPathQuery

        static readonly char[] SlashChar = new[] { '/' };

        #endregion

        #region Detached

        public IPublishedProperty CreateDetachedProperty(PublishedPropertyType propertyType, object value, bool isPreviewing)
        {
            if (propertyType.IsDetachedOrNested == false)
                throw new ArgumentException("Property type is neither detached nor nested.", "propertyType");
            return new XmlPublishedProperty(propertyType, isPreviewing, value.ToString());
        }

        #endregion
    }
}