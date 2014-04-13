using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using umbraco.NodeFactory;
using Umbraco.Core;

namespace umbraco
{
	/// <summary>
	/// uQuery sub-class for Nodes
	/// </summary>
	public static partial class uQuery
	{
		/// <summary>
		/// Gets the Root Node Id (-1)
		/// </summary>
		public static readonly int RootNodeId = -1;

		/// <summary>
		/// Get a collection of Umbraco Nodes from an XPath expression
		/// </summary>
		/// <param name="xpath">XPath expression to get Nodes, can use $ancestorOrSelf which will use the current Node if published, else it'll use the nearest published parent
		/// $currentPage will be depreciated</param>
		/// <returns>an empty collection or a collection of nodes</returns>
		public static IEnumerable<Node> GetNodesByXPath(string xpath)
		{
			var nodes = new List<Node>();

			// resolve the XPath expression with any Umbraco-specific parameters
			xpath = uQuery.ResolveXPath(xpath);

			// Get Umbraco Xml
			var xPathNavigator = content.Instance.XmlContent.CreateNavigator();
			XPathExpression xPathExpression;

			// Check to see if XPathExpression is in the cache
            if (HttpRuntime.Cache[xpath] == null)
            {
				// Build Compiled XPath expression
				xPathExpression = xPathNavigator.Compile(xpath);

				// Store in Cache
				HttpRuntime.Cache[xpath] = xPathExpression;
			}
			else // Get from Cache
			{
				xPathExpression = (XPathExpression)HttpRuntime.Cache[xpath];
			}

			// [LK] Interested in exploring options to call custom extension methods in XPath expressions.
			// http://msdn.microsoft.com/en-us/library/ms950806.aspx
			// http://msdn.microsoft.com/en-us/library/dd567715.aspx
			// Alternative is to render a Clean.xslt with the desired XPath, returning nodeIds

			var xPathNodeIterator = xPathNavigator.Select(xPathExpression);

			while (xPathNodeIterator.MoveNext())
			{
				var node = uQuery.GetNode(xPathNodeIterator.Current.GetAttribute("id", string.Empty));
				if (node != null)
				{
					nodes.Add(node);
				}
			}

			return nodes;
		}

		/// <summary>
		/// Returns a collection of Nodes, from a delimited list of Ids (as per the format used with UltimatePicker)
		/// </summary>
		/// <param name="csv">string csv of Ids</param>
		/// <returns>an empty collection or a collection or Nodes</returns>
		public static IEnumerable<Node> GetNodesByCsv(string csv)
		{
			var nodes = new List<Node>();
			var ids = uQuery.GetCsvIds(csv);

			if (ids != null)
			{
				foreach (string id in ids)
				{
					var node = uQuery.GetNode(id);
					if (node != null)
					{
						nodes.Add(node);
					}
				}
			}

			return nodes;
		}

		/// <summary>
		/// Builds a node collection from an XML snippet
		/// </summary>
		/// <param name="xml">
		/// the expected Xml snippet is that stored by the Multi-Node Tree Picker (and XPathCheckBoxList when storing Ids)
		/// "<MultiNodePicker>
		///     <nodeId>1065</nodeId>
		///     <nodeId>1068</nodeId>
		///     <nodeId>1066</nodeId>
		///  </MultiNodePicker>"
		/// </param>
		/// <returns>an empty list or a list of nodes</returns>
		public static IEnumerable<Node> GetNodesByXml(string xml)
		{
			var nodes = new List<Node>();
			var ids = uQuery.GetXmlIds(xml);

			if (ids != null)
			{
				foreach (int id in ids)
				{
					var node = uQuery.GetNode(id);
					if (node != null)
					{
						nodes.Add(node);
					}
				}
			}

			return nodes;
		}

		/// <summary>
		/// Get nodes by name
		/// </summary>
		/// <param name="name">name of node to look for</param>
		/// <returns>list of nodes, or empty list</returns>
		public static IEnumerable<Node> GetNodesByName(string name)
		{
			return uQuery.GetNodesByXPath(string.Concat("descendant::*[@nodeName='", name, "']"));
		}

		/// <summary>
		/// Get nodes by document type alias
		/// </summary>
		/// <param name="documentTypeAlias">The document type alias</param>
		/// <returns>list of nodes, or empty list</returns>
		public static IEnumerable<Node> GetNodesByType(string documentTypeAlias)
		{
			if (uQuery.IsLegacyXmlSchema())
			{
				return uQuery.GetNodesByXPath(string.Concat("descendant::*[@nodeTypeAlias='", documentTypeAlias, "']"));
			}
			else
			{
				return uQuery.GetNodesByXPath(string.Concat("descendant::", documentTypeAlias, "[@isDoc]"));
			}
		}

		/// <summary>
		/// Get nodes by document type id
		/// </summary>
		/// <param name="documentTypeId">The document type id.</param>
		/// <returns></returns>
		public static IEnumerable<Node> GetNodesByType(int documentTypeId)
		{
			return uQuery.GetNodesByXPath(string.Concat("descendant::*[@nodeType='", documentTypeId, "']"));
		}

		/// <summary>
		/// Gets the node by URL.
		/// </summary>
		/// <param name="url">url to search for</param>
		/// <returns>null or node matching supplied url</returns>
		public static Node GetNodeByUrl(string url)
		{
			return uQuery.GetNode(uQuery.GetNodeIdByUrl(url));
		}

		/// <summary>
		/// Gets the node id by path level.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="level">The level.</param>
		/// <returns>Returns the node id for a given path level.</returns>
		public static string GetNodeIdByPathLevel(string path, int level)
		{
			// TODO: [LK] use uQuery.GetNodeIdByPathLevel
			var nodeIds = path.Split(',').ToList();

			if (nodeIds.Count > level)
			{
				return nodeIds[level];
			}

			return uQuery.RootNodeId.ToString();
		}

		/// <summary>
		/// Gets the node Id by URL.
		/// </summary>
		/// <param name="url">The URL to get the XML node from.</param>
		/// <returns>Returns the node Id.</returns>
		/// <remarks>
		/// <para>Thanks to Jonas Eriksson http://our.umbraco.org/member/4853 </para>
		/// <para>Just runs lookups to find the document, does not follow internal redirects, 404 handlers,
		/// page access verification, wildcard domains -- nothing.</para>
		/// </remarks>
		public static int GetNodeIdByUrl(string url)
		{
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			if (!uri.IsAbsoluteUri)
				uri = uri.MakeAbsolute(Umbraco.Web.UmbracoContext.Current.CleanedUmbracoUrl);
			uri = Umbraco.Web.UriUtility.UriToUmbraco(uri);

			var pcr = new Umbraco.Web.Routing.PublishedContentRequest(uri, Umbraco.Web.UmbracoContext.Current.RoutingContext);
			// partially prepare the request: do _not_ follow redirects, handle 404, nothing - just find a content
			pcr.Engine.FindDomain();
			pcr.Engine.FindPublishedContent();
			return pcr.HasPublishedContent ? pcr.PublishedContent.Id : uQuery.RootNodeId;
		}

		/// <summary>
		/// Get top level content node
		/// </summary>
		/// <returns>the top level content node</returns>
		public static Node GetRootNode()
		{
			return new Node(RootNodeId);
		}

		/// <summary>
		/// checks to see if the current node can be got via the nodeFactory, if not then 
		/// checks to see if the current node can be got via an id on the QueryString
		/// </summary>
		/// <returns>the current node or null if not found</returns>
		public static Node GetCurrentNode()
		{
			Node currentNode = null;

			try
			{
				currentNode = Node.GetCurrent();
			}
			catch // if current node can't be found via the nodeFactory then Umbraco throws an exception
			{
				// look on QueryString for an id parameter (this is used in the backoffice)
				currentNode = uQuery.GetNode(uQuery.GetIdFromQueryString());
			}

			return currentNode;
		}

		/// <summary>
		/// Checks the supplied string can be cast to an integer, and returns the node with that Id
		/// </summary>
		/// <param name="nodeId">string representing the nodeId to return</param>
		/// <returns>Node or null</returns>
		public static Node GetNode(string nodeId)
		{
			int id;
			Node node = null;

			if (int.TryParse(nodeId, out id))
			{
				node = uQuery.GetNode(id);
			}

			return node;
		}

		/// <summary>
		/// Wrapper for Node constructor
		/// </summary>
		/// <param name="id">id of Node to get</param>
		/// <returns>Node or null</returns>
		public static Node GetNode(int id)
		{
			Node node;

			try
			{
				node = new Node(id);

				if (node.Id == 0)
				{
					node = null;
				}
			}
			catch
			{
				node = null;
			}

			return node;
		}

		/// <summary>
		/// Resolves the XPath expression with any Umbraco-specific parameters.
		/// </summary>
		/// <param name="xpath">The xpath expression.</param>
		/// <returns>Returns an XPath expression with the Umbraco-specific parameters resolved.</returns>
		public static string ResolveXPath(string xpath)
		{
			if (!string.IsNullOrWhiteSpace(xpath))
			{
				var parameters = new[] { "$currentPage", "$ancestorOrSelf", "$parentPage" };

				if (parameters.Any(xpath.Contains))
				{
					var ancestorOrSelfId = RootNodeId;
					var parentPageId = RootNodeId;

					var currentNode = uQuery.GetCurrentNode();
					if (currentNode != null)
					{
						ancestorOrSelfId = currentNode.Id;
						parentPageId = (currentNode.Parent != null) ? currentNode.Parent.Id : currentNode.Id;
					}
					else
					{
						// current node is unpublished or can't be found, so try via the Document API
						var currentDocument = uQuery.GetCurrentDocument();
						if (currentDocument != null)
						{
							// need to find first published parent
							var publishedDocument = currentDocument.GetAncestorOrSelfDocuments().Where(document => document.Published == true).FirstOrDefault();
							if (publishedDocument != null)
							{
								// found the nearest published document
								ancestorOrSelfId = publishedDocument.Id;
								parentPageId = (publishedDocument.Id == currentDocument.Id) ? currentDocument.ParentId : publishedDocument.Id;
							}
						}
					}

					// replace the parameters with corresponding XPath expression
					xpath = xpath.Replace("$parentPage", string.Concat("/descendant::*[@id='", parentPageId, "']"));
					xpath = xpath.Replace("$currentPage", "$ancestorOrSelf");
					xpath = xpath.Replace("$ancestorOrSelf", string.Concat("/descendant::*[@id='", ancestorOrSelfId, "']"));
				}
			}

			return xpath;
		}

		/// <summary>
		/// Extension method on Node collection to return key value pairs of: node.Id / node.Name
		/// </summary>
		/// <param name="nodes">generic list of node objects</param>
		/// <returns>a collection of nodeIDs and their names</returns>
		public static Dictionary<int, string> ToNameIds(this IEnumerable<Node> nodes)
		{
			var dictionary = new Dictionary<int, string>();

			foreach (var node in nodes)
			{
				dictionary.Add(node.Id, node.Name);
			}

			return dictionary;
		}
	}
}