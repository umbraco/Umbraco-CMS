using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Xml;
using Umbraco.Web.Routing;
using umbraco;
using System.Linq;
using umbraco.BusinessLogic;
using umbraco.presentation.preview;

namespace Umbraco.Web.PublishedCache.LegacyXmlCache
{
    internal class PublishedContentCache : IPublishedContentCache
    {
        #region Routes cache

        private readonly RoutesCache _routesCache = new RoutesCache(!UnitTesting);

        // for INTERNAL, UNIT TESTS use ONLY
        internal RoutesCache RoutesCache { get { return _routesCache; } }

        // for INTERNAL, UNIT TESTS use ONLY
        internal static bool UnitTesting = false;

        /// <summary>
        /// Gets content identified by a route.
        /// </summary>
        /// <param name="umbracoContext">The context.</param>
        /// <param name="route">The route</param>
        /// <param name="hideTopLevelNode">A value forcing the HideTopLevelNode setting.</param>
        /// <returns>The content, or null.</returns>
        /// <remarks>
        /// <para>A valid route is either a simple path eg <c>/foo/bar/nil</c> or a root node id and a path, eg <c>123/foo/bar/nil</c>.</para>
        /// <para>If <param name="hideTopLevelNode" /> is <c>null</c> then the settings value is used.</para>
        /// </remarks>
        public IPublishedContent GetByRoute(UmbracoContext umbracoContext, string route, bool? hideTopLevelNode = null)
        {
            if (route == null) throw new ArgumentNullException("route");

            // try to get from cache if not previewing
            var contentId = umbracoContext.InPreviewMode
                ? 0
                : _routesCache.GetNodeId(route);

            // if found id in cache then get corresponding content
            // and clear cache if not found - for whatever reason
            IPublishedContent content = null;
            if (contentId > 0)
            {
                content = GetById(umbracoContext, contentId);
                if (content == null)
                    _routesCache.ClearNode(contentId);
            }

            // still have nothing? actually determine the id
            hideTopLevelNode = hideTopLevelNode ?? GlobalSettings.HideTopLevelNodeFromPath; // default = settings
            content = content ?? DetermineIdByRoute(umbracoContext, route, hideTopLevelNode.Value);

            // cache if we have a content and not previewing
            if (content != null && !umbracoContext.InPreviewMode)
            {
                var domainRootNodeId = route.StartsWith("/") ? -1 : int.Parse(route.Substring(0, route.IndexOf('/')));
                var iscanon = !UnitTesting && !DomainHelper.ExistsDomainInPath(DomainHelper.GetAllDomains(false), content.Path, domainRootNodeId);
                // and only if this is the canonical url (the one GetUrl would return)
                if (iscanon)
                    _routesCache.Store(contentId, route);
            }

            return content;
        }

        /// <summary>
        /// Gets the route for a content identified by its unique identifier.
        /// </summary>
        /// <param name="umbracoContext">The context.</param>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The route.</returns>
        public string GetRouteById(UmbracoContext umbracoContext, int contentId)
        {
            // try to get from cache if not previewing
            var route = umbracoContext.InPreviewMode
                ? null
                : _routesCache.GetRoute(contentId);

            // if found in cache then return
            if (route != null)
                return route;

            // else actually determine the route
            route = DetermineRouteById(umbracoContext, contentId);

            // cache if we have a route and not previewing
            if (route != null && !umbracoContext.InPreviewMode)
                _routesCache.Store(contentId, route);

            return route;
        }

        IPublishedContent DetermineIdByRoute(UmbracoContext umbracoContext, string route, bool hideTopLevelNode)
        {
            if (route == null) throw new ArgumentNullException("route");

            //the route always needs to be lower case because we only store the urlName attribute in lower case
            route = route.ToLowerInvariant();

            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);
            var startNodeId = pos == 0 ? 0 : int.Parse(route.Substring(0, pos));
            IEnumerable<XPathVariable> vars;

            var xpath = CreateXpathQuery(startNodeId, path, hideTopLevelNode, out vars);

            //check if we can find the node in our xml cache
            var content = GetSingleByXPath(umbracoContext, xpath, vars == null ? null : vars.ToArray());

            // if hideTopLevelNodePath is true then for url /foo we looked for /*/foo
            // but maybe that was the url of a non-default top-level node, so we also
            // have to look for /foo (see note in ApplyHideTopLevelNodeFromPath).
            if (content == null && hideTopLevelNode && path.Length > 1 && path.IndexOf('/', 1) < 0)
            {
                xpath = CreateXpathQuery(startNodeId, path, false, out vars);
                content = GetSingleByXPath(umbracoContext, xpath, vars == null ? null : vars.ToArray());
            }

            return content;
        }

        string DetermineRouteById(UmbracoContext umbracoContext, int contentId)
        {
            var node = GetById(umbracoContext, contentId);
            if (node == null)
                return null;

            // walk up from that node until we hit a node with a domain,
            // or we reach the content root, collecting urls in the way
            var pathParts = new List<string>();
            var n = node;
            var hasDomains = DomainHelper.NodeHasDomains(n.Id);
            while (!hasDomains && n != null) // n is null at root
            {
                // get the url
                var urlName = n.UrlName;
                pathParts.Add(urlName);

                // move to parent node
                n = n.Parent;
                hasDomains = n != null && DomainHelper.NodeHasDomains(n.Id);
            }

            // no domain, respect HideTopLevelNodeFromPath for legacy purposes
            if (!hasDomains && global::umbraco.GlobalSettings.HideTopLevelNodeFromPath)
                ApplyHideTopLevelNodeFromPath(umbracoContext, node, pathParts);

            // assemble the route
            pathParts.Reverse();
            var path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
            var route = (n == null ? "" : n.Id.ToString(CultureInfo.InvariantCulture)) + path;

            return route;
        }

        static void ApplyHideTopLevelNodeFromPath(UmbracoContext umbracoContext, IPublishedContent node, IList<string> pathParts)
        {
            // in theory if hideTopLevelNodeFromPath is true, then there should be only once
            // top-level node, or else domains should be assigned. but for backward compatibility
            // we add this check - we look for the document matching "/" and if it's not us, then
            // we do not hide the top level path
            // it has to be taken care of in GetByRoute too so if
            // "/foo" fails (looking for "/*/foo") we try also "/foo". 
            // this does not make much sense anyway esp. if both "/foo/" and "/bar/foo" exist, but
            // that's the way it works pre-4.10 and we try to be backward compat for the time being
            if (node.Parent == null)
            {
                var rootNode = umbracoContext.ContentCache.GetByRoute("/", true);
                if (rootNode.Id == node.Id) // remove only if we're the default node
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

			public static string Root { get { return "/root"; } }
			public string RootDocuments { get; private set; }
			public string DescendantDocumentById { get; private set; }
			public string DescendantDocumentByAlias { get; private set; }
			public string ChildDocumentByUrlName { get; private set; }
            public string ChildDocumentByUrlNameVar { get; private set; }
            public string RootDocumentWithLowestSortOrder { get; private set; }

			public XPathStringsDefinition(int version)
			{
				Version = version;

				switch (version)
				{
					// legacy XML schema
					case 0:
						RootDocuments = "/root/node";
						DescendantDocumentById = "//node [@id={0}]";
						DescendantDocumentByAlias = "//node[("
							+ "contains(concat(',',translate(data [@alias='umbracoUrlAlias'], ' ', ''),','),',{0},')"
							+ " or contains(concat(',',translate(data [@alias='umbracoUrlAlias'], ' ', ''),','),',/{0},')"
							+ ")]";
						ChildDocumentByUrlName = "/node [@urlName='{0}']";
						ChildDocumentByUrlNameVar = "/node [@urlName=${0}]";
						RootDocumentWithLowestSortOrder = "/root/node [not(@sortOrder > ../node/@sortOrder)][1]";
						break;

					// default XML schema as of 4.10
					case 1:
						RootDocuments = "/root/* [@isDoc]";
						DescendantDocumentById = "//* [@isDoc and @id={0}]";
						DescendantDocumentByAlias = "//* [@isDoc and ("
							+ "contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',{0},')"
							+ " or contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',/{0},')"
							+ ")]";
						ChildDocumentByUrlName = "/* [@isDoc and @urlName='{0}']";
						ChildDocumentByUrlNameVar = "/* [@isDoc and @urlName=${0}]";
						RootDocumentWithLowestSortOrder = "/root/* [@isDoc and not(@sortOrder > ../* [@isDoc]/@sortOrder)][1]";
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

				var version = UmbracoSettings.UseLegacyXmlSchema ? 0 : 1;
				if (_xPathStringsValue == null || _xPathStringsValue.Version != version)
					_xPathStringsValue = new XPathStringsDefinition(version);
				return _xPathStringsValue;
			}
		}

		#endregion

        #region Converters

        private static IPublishedContent ConvertToDocument(XmlNode xmlNode)
		{
		    return xmlNode == null ? null : new Models.XmlPublishedContent(xmlNode);
		}   

        private static IEnumerable<IPublishedContent> ConvertToDocuments(XmlNodeList xmlNodes)
        {
            return xmlNodes.Cast<XmlNode>().Select(xmlNode => new Models.XmlPublishedContent(xmlNode));
        }

        #endregion

        #region Getters

        public virtual IPublishedContent GetById(UmbracoContext umbracoContext, int nodeId)
    	{
    		return ConvertToDocument(GetXml(umbracoContext).GetElementById(nodeId.ToString()));
    	}

		public IEnumerable<IPublishedContent> GetAtRoot(UmbracoContext umbracoContext)
		{
			return (from XmlNode x in GetXml(umbracoContext).SelectNodes(XPathStrings.RootDocuments) select ConvertToDocument(x)).ToList();
		}

        public IPublishedContent GetSingleByXPath(UmbracoContext umbracoContext, string xpath, params XPathVariable[] vars)
        {
            if (xpath == null) throw new ArgumentNullException("xpath");
            if (string.IsNullOrWhiteSpace(xpath)) return null;

            var xml = GetXml(umbracoContext);
            var node = vars == null
                ? xml.SelectSingleNode(xpath)
                : xml.SelectSingleNode(xpath, vars);
            return ConvertToDocument(node);
        }

        public IEnumerable<IPublishedContent> GetByXPath(UmbracoContext umbracoContext, string xpath, params XPathVariable[] vars)
        {
            if (xpath == null) throw new ArgumentNullException("xpath");
            if (string.IsNullOrWhiteSpace(xpath)) return Enumerable.Empty<IPublishedContent>();

            var xml = GetXml(umbracoContext);
            var nodes = vars == null
                ? xml.SelectNodes(xpath)
                : xml.SelectNodes(xpath, vars);
            return ConvertToDocuments(nodes);
        }
        
        // FIXME MOVE THAT ONE OUT OF HERE?
        public IPublishedContent GetByUrlAlias(UmbracoContext umbracoContext, int rootNodeId, string alias)
        {
			if (alias == null) throw new ArgumentNullException("alias");

			// the alias may be "foo/bar" or "/foo/bar"
            // there may be spaces as in "/foo/bar,  /foo/nil"
            // these should probably be taken care of earlier on

            alias = alias.TrimStart('/');
            var xpathBuilder = new StringBuilder();
            xpathBuilder.Append(XPathStringsDefinition.Root);

			if (rootNodeId > 0)
				xpathBuilder.AppendFormat(XPathStrings.DescendantDocumentById, rootNodeId);

            XPathVariable var = null;
            if (alias.Contains('\'') || alias.Contains('"'))
            {
                // use a var, escaping gets ugly pretty quickly
                var = new XPathVariable("alias", alias);
                alias = "$alias";
            }
			xpathBuilder.AppendFormat(XPathStrings.DescendantDocumentByAlias, alias);

			var xpath = xpathBuilder.ToString();

		    return GetSingleByXPath(umbracoContext, xpath, var);
        }

        public bool HasContent()
        {
	        var xml = GetXml();
			if (xml == null)
				return false;
			var node = xml.SelectSingleNode(XPathStrings.RootDocuments);
			return node != null;
        }

        #endregion

        #region Legacy Xml

        private PreviewContent _previewContent;
        private Func<User, bool, XmlDocument> _xmlDelegate;

        /// <summary>
        /// Gets/sets the delegate used to retreive the Xml content, generally the setter is only used for unit tests
        /// and by default if it is not set will use the standard delegate which ONLY works when in the context an Http Request
        /// </summary>
        /// <remarks>
        /// If not defined, we will use the standard delegate which ONLY works when in the context an Http Request
        /// mostly because the 'content' object heavily relies on HttpContext, SQL connections and a bunch of other stuff
        /// that when run inside of a unit test fails.
        /// </remarks>
        internal Func<User, bool, XmlDocument> GetXmlDelegate
        {
            get
            {
                return _xmlDelegate ?? (_xmlDelegate = (user, preview) =>
                {
                    if (preview)
                    {
                        if (_previewContent == null)
                        {
                            _previewContent = new PreviewContent(user, new Guid(global::umbraco.BusinessLogic.StateHelper.Cookies.Preview.GetValue()), true);
                            if (_previewContent.ValidPreviewSet)
                                _previewContent.LoadPreviewset();
                        }
                        if (_previewContent.ValidPreviewSet)
                            return _previewContent.XmlContent;
                    }
                    return content.Instance.XmlContent;
                });
            }
            set
            {
                _xmlDelegate = value;
            }
        }

        internal XmlDocument GetXml(UmbracoContext umbracoContext)
        {
            return GetXmlDelegate(umbracoContext.UmbracoUser, umbracoContext.InPreviewMode);
        }

        internal XmlDocument GetXml()
        {
            return GetXmlDelegate(null, false);
        }

        #endregion

        #region XPathQuery

        static readonly char[] SlashChar = new[] { '/' };

        protected string CreateXpathQuery(int startNodeId, string path, bool hideTopLevelNodeFromPath, out IEnumerable<XPathVariable> vars)
        {
            string xpath;
            vars = null;

            if (path == string.Empty || path == "/")
            {
                // if url is empty
                if (startNodeId > 0)
                {
					// if in a domain then use the root node of the domain
					xpath = string.Format(XPathStringsDefinition.Root + XPathStrings.DescendantDocumentById, startNodeId);                    
                }
                else
                {
                    // if not in a domain - what is the default page?
                    // let's say it is the first one in the tree, if any -- order by sortOrder

					// but!
					// umbraco does not consistently guarantee that sortOrder starts with 0
					// so the one that we want is the one with the smallest sortOrder
					// read http://stackoverflow.com/questions/1128745/how-can-i-use-xpath-to-find-the-minimum-value-of-an-attribute-in-a-set-of-elemen
                    
					// so that one does not work, because min(@sortOrder) maybe 1
					// xpath = "/root/*[@isDoc and @sortOrder='0']";

					// and we can't use min() because that's XPath 2.0
					// that one works
					xpath = XPathStrings.RootDocumentWithLowestSortOrder;
                }
            }
            else
            {
                // if url is not empty, then use it to try lookup a matching page
                var urlParts = path.Split(SlashChar, StringSplitOptions.RemoveEmptyEntries);
                var xpathBuilder = new StringBuilder();
                int partsIndex = 0;
                List<XPathVariable> varsList = null;

                if (startNodeId == 0)
                {
					if (hideTopLevelNodeFromPath)
						xpathBuilder.Append(XPathStrings.RootDocuments); // first node is not in the url
					else
						xpathBuilder.Append(XPathStringsDefinition.Root);
                }
                else
                {
					xpathBuilder.AppendFormat(XPathStringsDefinition.Root + XPathStrings.DescendantDocumentById, startNodeId);
					// always "hide top level" when there's a domain
                }

                while (partsIndex < urlParts.Length)
                {
                    var part = urlParts[partsIndex++];
                    if (part.Contains('\'') || part.Contains('"'))
                    {
                        // use vars, escaping gets ugly pretty quickly
                        varsList = varsList ?? new List<XPathVariable>();
                        var varName = string.Format("var{0}", partsIndex);
                        varsList.Add(new XPathVariable(varName, part));
                        xpathBuilder.AppendFormat(XPathStrings.ChildDocumentByUrlNameVar, varName);
                    }
                    else
                    {
                        xpathBuilder.AppendFormat(XPathStrings.ChildDocumentByUrlName, part);
                        
                    }
                }

                xpath = xpathBuilder.ToString();
                if (varsList != null)
                    vars = varsList.ToArray();
            }

            return xpath;
        }

        #endregion
    }
}