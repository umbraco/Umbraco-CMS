using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
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
                _routesCache.Store(content.Id, route, true); // trusted route
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

            // cache the route BUT do NOT trust it as it can be a colliding route
            // meaning if we GetRouteById again, we'll get it from cache, but it
            // won't be used for inbound routing
            if (preview == false)
                _routesCache.Store(contentId, route, false);

            return route;
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
            return id > 0 ? GetById(umbracoContext, preview, id) : null;
        }

        private static XmlElement GetXmlElementChildWithLowestSortOrder(XmlNode element)
        {
            XmlElement elt = null;
            var min = int.MaxValue;
            foreach (var n in element.ChildNodes)
            {
                var e = n as XmlElement;
                if (e == null) continue;

                var sortOrder = int.Parse(e.GetAttribute("sortOrder"));
                if (sortOrder >= min) continue;

                min = sortOrder;
                elt = e;
            }
            return elt;
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

                elt = GetXmlElementChildWithLowestSortOrder(xml.DocumentElement);
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
                //Don't use OfType<T> or Cast<T>, this is critical code, all ChildNodes are XmlElement so explicitly cast
                // https://gist.github.com/Shazwazza/04e2e5642a316f4a87e52dada2901198
                foreach (var n in elt.ChildNodes)
                {
                    var e = n as XmlElement;
                    if (e == null) continue;

                    var id = NavigateElementRoute(e, urlParts);
                    if (id > 0) return id;
                }

                if (urlParts.Length > 1)
                    return -1;
            }

            return NavigateElementRoute(elt, urlParts);
        }

        private static bool UseLegacySchema
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema; }
        }

        private static int NavigateElementRoute(XmlElement elt, string[] urlParts)
        {
            var found = true;
            var i = 0;
            while (found && i < urlParts.Length)
            {
                found = false;
                //Don't use OfType<T> or Cast<T>, this is critical code, all ChildNodes are XmlElement so explicitly cast
                // https://gist.github.com/Shazwazza/04e2e5642a316f4a87e52dada2901198
                var sortOrder = -1;
                foreach (var o in elt.ChildNodes)
                {
                    var child = o as XmlElement;
                    if (child == null) continue;

                    var noNode = UseLegacySchema
                        ? child.Name != "node"
                        : child.GetAttributeNode("isDoc") == null;
                    if (noNode) continue;
                    if (child.GetAttribute("urlName") != urlParts[i]) continue;

                    found = true;

                    var so = int.Parse(child.GetAttribute("sortOrder"));
                    if (sortOrder >= 0 && so >= sortOrder) continue;

                    sortOrder = so;
                    elt = child;
                }
                i++;
            }
            return found ? int.Parse(elt.GetAttribute("id")) : -1;
        }

        string DetermineRouteById(UmbracoContext umbracoContext, bool preview, int contentId)
        {
            var xml = GetXml(umbracoContext, preview);
            var elt = xml.GetElementById(contentId.ToString(CultureInfo.InvariantCulture));
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
                id = int.Parse(e.GetAttribute("id"), CultureInfo.InvariantCulture);
                hasDomains = id != -1 && domainHelper.NodeHasDomains(id);
            }

            // no domain, respect HideTopLevelNodeFromPath for legacy purposes
            if (hasDomains == false && GlobalSettings.HideTopLevelNodeFromPath)
            {
                if (eltParentId == -1)
                {
                    var rootNode = GetXmlElementChildWithLowestSortOrder(xml.DocumentElement);
                    if (rootNode != null && rootNode == elt)
                        pathParts.RemoveAt(pathParts.Count - 1);
                }
                else
                {
                    pathParts.RemoveAt(pathParts.Count - 1);
                }
            }

            // assemble the route
            pathParts.Reverse();
            var path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
            var route = (id == -1 ? "" : id.ToString(CultureInfo.InvariantCulture)) + path;

            return route;
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

        private readonly object _idkMapLocker = new object();
        private IdkMap _idkMap;

        // populate the idkmap by indexing the content cache
        // assuming that the content cache cannot be corrupted
        private void EnsureIdkMap(UmbracoContext umbracoContext)
        {
            lock (_idkMapLocker)
            {
                if (_idkMap != null) return;

                _idkMap = ApplicationContext.Current.Services.IdkMap; // fixme inject

                // give the map a fast mapper
                _idkMap.SetMapper(UmbracoObjectTypes.Document, GetKeyForId, GetIdForKey);

                // populate the map with what we know, so far
                var xml = GetXml(umbracoContext, false);
                var nav = xml.CreateNavigator();
                var iter = nav.SelectDescendants(XPathNodeType.Element, true);
                _idkMap.Populate(Enumerate(iter), UmbracoObjectTypes.Document);
            }
        }

        private IEnumerable<(int id, Guid key)> Enumerate(XPathNodeIterator iter)
        {
            while (iter.MoveNext())
            {
                string idString = null;
                string keyString = null;

                if (iter.Current.MoveToFirstAttribute())
                {
                    do
                    {
                        switch (iter.Current.Name)
                        {
                            case "id":
                                idString = iter.Current.Value;
                                break;
                            case "key":
                                keyString = iter.Current.Value;
                                break;
                        }
                    } while ((idString == null || keyString == null) && iter.Current.MoveToNextAttribute());

                    iter.Current.MoveToParent();
                }

                if (idString == null || keyString == null) continue;

                var id = int.Parse(idString);
                var key = Guid.Parse(keyString);
                yield return (id, key);
            }
        }

        private Guid GetKeyForId(int id)
        {
            var xml = GetXml(UmbracoContext.Current, false);
            var elt = xml.GetElementById(id.ToString(CultureInfo.InvariantCulture));
            return elt == null ? default (Guid) : Guid.Parse(elt.GetAttribute("key"));
        }

        private int GetIdForKey(Guid key)
        {
            var xml = GetXml(UmbracoContext.Current, false);
            var elt = xml.SelectSingleNode("//* [@key=$guid]", new XPathVariable("guid", key.ToString())) as XmlElement;
            return elt == null ? default (int) : int.Parse(elt.GetAttribute("id"));
        }

        public virtual IPublishedContent GetById(UmbracoContext umbracoContext, bool preview, int nodeId)
    	{
    		return ConvertToDocument(GetXml(umbracoContext, preview).GetElementById(nodeId.ToString(CultureInfo.InvariantCulture)), preview);
    	}

        public virtual IPublishedContent GetById(UmbracoContext umbracoContext, bool preview, Guid nodeKey)
        {
            EnsureIdkMap(umbracoContext);
            var mapAttempt = _idkMap.GetIdForKey(nodeKey, UmbracoObjectTypes.Document);
            return mapAttempt ? GetById(umbracoContext, preview, mapAttempt.Result) : null;
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
                        if (PreviewContent.IsSinglePreview)
                            return content.Instance.PreviewXmlContent;
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
