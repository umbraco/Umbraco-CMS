using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using umbraco;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;

namespace umbraco
{
	/// <summary>
	/// uQuery extensions for the Node object.
	/// </summary>
	public static class NodeExtensions
	{
		/// <summary>
		/// Gets the ancestor by path level.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="level">The level.</param>
		/// <returns>Returns an ancestor node by path level.</returns>
		public static Node GetAncestorByPathLevel(this Node node, int level)
		{
			var nodeId = uQuery.GetNodeIdByPathLevel(node.Path, level);
			return uQuery.GetNode(nodeId);
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'ancestor'
		/// Get the Ancestor Nodes from current to root, (useful for breadcrumbs)
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <returns>Node as IEnumerable</returns>
		public static IEnumerable<Node> GetAncestorNodes(this Node node)
		{
			var ancestor = node.Parent;

			while (ancestor != null)
			{
				yield return ancestor as Node;

				ancestor = ancestor.Parent;
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'ancestor-or-self'
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <returns>Node as IEnumerable</returns>
		public static IEnumerable<Node> GetAncestorOrSelfNodes(this Node node)
		{
			yield return node;

			foreach (var ancestor in node.GetAncestorNodes())
			{
				yield return ancestor;
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'preceding-sibling'
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <returns>Node as IEumerable</returns>
		public static IEnumerable<Node> GetPrecedingSiblingNodes(this Node node)
		{
			if (node.Parent != null)
			{
				var parentNode = node.Parent as Node;
				foreach (var precedingSiblingNode in parentNode.GetChildNodes().Where(childNode => childNode.SortOrder < node.SortOrder))
				{
					yield return precedingSiblingNode;
				}
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'following-sibling'
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <returns>Node as IEumerable</returns>
		public static IEnumerable<Node> GetFollowingSiblingNodes(this Node node)
		{
			if (node.Parent != null)
			{
				var parentNode = node.Parent as Node;
				foreach (var followingSiblingNode in parentNode.GetChildNodes().Where(childNode => childNode.SortOrder > node.SortOrder))
				{
					yield return followingSiblingNode;
				}
			}
		}

		/// <summary>
		/// Gets all sibling Nodes
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <returns>Node as IEumerable</returns>
		public static IEnumerable<Node> GetSiblingNodes(this Node node)
		{
			if (node.Parent != null)
			{
				var parentNode = node.Parent as Node;
				foreach (var siblingNode in parentNode.GetChildNodes().Where(childNode => childNode.Id != node.Id))
				{
					yield return siblingNode;
				}
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'descendant-or-self'
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <returns>Node as IEnumerable</returns>
		public static IEnumerable<Node> GetDescendantOrSelfNodes(this Node node)
		{
			yield return node;

			foreach (var descendant in node.GetDescendantNodes())
			{
				yield return descendant;
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'descendant'
		/// Make the All Descendants LINQ queryable
		/// taken from: http://our.umbraco.org/wiki/how-tos/useful-helper-extension-methods-%28linq-null-safe-access%29
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <returns>Node as IEnumerable</returns>
		public static IEnumerable<Node> GetDescendantNodes(this Node node)
		{
			foreach (Node child in node.Children)
			{
				yield return child;

				foreach (var descendant in child.GetDescendantNodes())
				{
					yield return descendant;
				}
			}
		}

		/// <summary>
		/// Drills down into the descendant nodes returning those where Func is true, when Func is false further descendants are not checked
		/// taken from: http://ucomponents.codeplex.com/discussions/246406
		/// </summary>
		/// <param name="node">The <c>umbraco.presentation.nodeFactory.Node</c>.</param>
		/// <param name="func">The func</param>
		/// <returns>Nodes as IEnumerable</returns>
		public static IEnumerable<Node> GetDescendantNodes(this Node node, Func<Node, bool> func)
		{
			foreach (Node child in node.Children)
			{
				if (func(child))
				{
					yield return child;

					foreach (var descendant in child.GetDescendantNodes(func))
					{
						yield return descendant;
					}
				}
			}
		}

		/// <summary>
		/// Gets the descendant nodes by document-type.
		/// </summary>
		/// <param name="node">The <c>umbraco.presentation.nodeFactory.Node</c>.</param>
		/// <param name="documentTypeAlias">The document type alias.</param>
		/// <returns>Nodes as IEnumerable</returns>
		public static IEnumerable<Node> GetDescendantNodesByType(this Node node, string documentTypeAlias)
		{
			return node.GetDescendantNodes(n => n.NodeTypeAlias == documentTypeAlias);
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'child'
		/// Make the imediate Children LINQ queryable
		/// Performance optimised for just imediate children.
		/// taken from: http://our.umbraco.org/wiki/how-tos/useful-helper-extension-methods-%28linq-null-safe-access%29
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <returns>Node as IEnumerable</returns>
		public static IEnumerable<Node> GetChildNodes(this Node node)
		{
			foreach (Node child in node.Children)
			{
				yield return child;
			}
		}

		/// <summary>
		/// Gets the child nodes.
		/// </summary>
		/// <param name="node">The <c>umbraco.presentation.nodeFactory.Node</c>.</param>
		/// <param name="func">The func.</param>
		/// <returns>Nodes as IEnumerable</returns>
		public static IEnumerable<Node> GetChildNodes(this Node node, Func<Node, bool> func)
		{
			foreach (Node child in node.Children)
			{
				if (func(child))
				{
					yield return child;
				}
			}
		}

		/// <summary>
		/// Gets the child nodes by document-type.
		/// </summary>
		/// <param name="node">The <c>umbraco.presentation.nodeFactory.Node</c>.</param>
		/// <param name="documentTypeAlias">The document type alias.</param>
		/// <returns>Nodes as IEnumerable</returns>
		public static IEnumerable<Node> GetChildNodesByType(this Node node, string documentTypeAlias)
		{
			return node.GetChildNodes(n => n.NodeTypeAlias == documentTypeAlias);
		}

		/// <summary>
		/// Extension method on Node to retun a matching child node by name
		/// </summary>
		/// <param name="parentNode">an umbraco.presentation.nodeFactory.Node object</param>
		/// <param name="nodeName">name of node to search for</param>
		/// <returns>null or Node</returns>
		public static Node GetChildNodeByName(this Node parentNode, string nodeName)
		{
			Node node = null;

			foreach (Node child in parentNode.Children)
			{
				if (child.Name == nodeName)
				{
					node = child;
					break;
				}
			}

			return node;

			//// return node.GetChildNodes(n => n.Name == nodeName);
		}

		/// <summary>
		/// Determines whether the specified node has property.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="propertyAlias">The property alias.</param>
		/// <returns>
		///   <c>true</c> if the specified node has property; otherwise, <c>false</c>.
		/// </returns>
		public static bool HasProperty(this Node node, string propertyAlias)
		{
			var property = node.GetProperty(propertyAlias);
			return (property != null);
		}

#pragma warning disable 0618
        /// <summary>
		/// Get a value of type T from a property
		/// </summary>
		/// <typeparam name="T">type T to cast to</typeparam>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <param name="propertyAlias">alias of property to get</param>
		/// <returns>default(T) or property cast to (T)</returns>
		public static T GetProperty<T>(this Node node, string propertyAlias)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(T));

			if (typeConverter != null)
			{
				if (typeof(T) == typeof(bool))
				{
                    // Use the GetPropertyAsBoolean method, as this handles true also being stored as "1"
                    return (T)typeConverter.ConvertFrom(node.GetPropertyAsBoolean(propertyAlias).ToString());
                }

				try
				{
					return (T)typeConverter.ConvertFromString(node.GetPropertyAsString(propertyAlias));
				}
				catch
				{
					return default(T);
				}
			}
			else
			{
				return default(T);
            }
        }
#pragma warning restore 0618

        /// <summary>
		/// Get a string value for the supplied property alias
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <param name="propertyAlias">alias of propety to get</param>
		/// <returns>empty string, or property value as string</returns>
		// TODO: [LK] Move to uComponents.Legacy project
		[Obsolete("Use .GetProperty<string>(propertyAlias) instead", false)]
		public static string GetPropertyAsString(this Node node, string propertyAlias)
		{
			var propertyValue = string.Empty;
			var property = node.GetProperty(propertyAlias);

			if (property != null)
			{
				propertyValue = property.Value;
			}

			return propertyValue;
		}

		/// <summary>
		/// Get a boolean value for the supplied property alias (works with built in Yes/No dataype)
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <param name="propertyAlias">alias of propety to get</param>
		/// <returns>true if can cast value, else false for all other circumstances</returns>
		// TODO: [LK] Move to uComponents.Legacy project
		[Obsolete("Use .GetProperty<bool>(propertyAlias) instead", false)]
        public static bool GetPropertyAsBoolean(this Node node, string propertyAlias)
		{
			var propertyValue = false;
			var property = node.GetProperty(propertyAlias);

			if (property != null)
			{
				// Umbraco yes / no datatype stores a string value of '1' or '0'
				if (property.Value == "1")
				{
					propertyValue = true;
				}
				else
				{
					bool.TryParse(property.Value, out propertyValue);
				}
			}

			return propertyValue;
		}

		/// <summary>
		/// Get a DateTime value for the supplied property alias
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <param name="propertyAlias">alias of propety to get</param>
		/// <returns>DateTime value or DateTime.MinValue for all other circumstances</returns>
		// TODO: [LK] Move to uComponents.Legacy project
		[Obsolete("Use .GetProperty<DateTime>(propertyAlias) instead", false)]
		public static DateTime GetPropertyAsDateTime(this Node node, string propertyAlias)
		{
			var propertyValue = DateTime.MinValue;
			var property = node.GetProperty(propertyAlias);

			if (property != null)
			{
				DateTime.TryParse(property.Value, out propertyValue);
			}

			return propertyValue;
		}

		/// <summary>
		/// Get an int value for the supplied property alias
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <param name="propertyAlias">alias of propety to get</param>
		/// <returns>int value of property or int.MinValue for all other circumstances</returns>    
		// TODO: [LK] Move to uComponents.Legacy project
		[Obsolete("Use .GetProperty<int>(propertyAlias) instead", false)]
		public static int GetPropertyAsInt(this Node node, string propertyAlias)
		{
			var propertyValue = int.MinValue;
			var property = node.GetProperty(propertyAlias);

			if (property != null)
			{
				int.TryParse(property.Value, out propertyValue);
			}

			return propertyValue;
		}

		/// <summary>
		/// Extension method on Node obj to get it's depth
		/// taken from: http://our.umbraco.org/wiki/how-tos/useful-helper-extension-methods-%28linq-null-safe-access%29
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <returns>int for depth, starts at 1</returns>
		// TODO: [LK] Move to uComponents.Legacy project
		[Obsolete("Use .Level instead")]
		public static int GetDepth(this Node node)
		{
			return node.Path.Split(',').ToList().Count;
		}

		/// <summary>
		/// Tell me the level of this node (0 = root)
		/// updated from Depth and changed to start at 0
		/// to align with other 'Level' methods (eg xslt)
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static int Level(this Node node)
		{
			return node.Path.Split(',').Length - 1;
		}

		/// <summary>
		/// Gets the XML for the Node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <returns>Returns an <c>XmlNode</c> for the selected Node</returns>
		public static XmlNode ToXml(this Node node)
		{
			return ((IHasXmlNode)umbraco.library.GetXmlNodeById(node.Id.ToString()).Current).GetNode();
		}

		/// <summary>
		/// Returns the url for a given crop name using the built in Image Cropper datatype
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <param name="propertyAlias">property alias</param>
		/// <param name="cropName">name of crop to get url for</param>
		/// <returns>emtpy string or url</returns>
		public static string GetImageCropperUrl(this Node node, string propertyAlias, string cropName)
		{
			string cropUrl = string.Empty;

			/*
			* Example xml : 
			* 
			* <crops date="28/11/2010 16:08:13">
			*   <crop name="Big" x="0" y="0" x2="1024" y2="768" url="/media/135/image_Big.jpg" />
			*   <crop name="Small" x="181" y="0" x2="608" y2="320" url="/media/135/image_Small.jpg" />
			* </crops>
			* 
			*/

			if (!string.IsNullOrEmpty(node.GetProperty<string>(propertyAlias)))
			{
				var xml = node.GetProperty<string>(propertyAlias);

				// TODO: [??] Add Node extension method: GetPropertyAsXmlNode()

				var xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(xml);

				var cropNode = xmlDocument.SelectSingleNode(string.Concat("descendant::crops/crop[@name='", cropName, "']"));

				if (cropNode != null)
				{
					cropUrl = cropNode.Attributes.GetNamedItem("url").InnerText;
				}
			}

			return cropUrl;
		}

		/// <summary>
		/// Sets a property value on this node
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <param name="propertyAlias">alias of property to set</param>
		/// <param name="value">value to set</param>
		/// <returns>the same node object on which this is an extension method</returns>
		public static Node SetProperty(this Node node, string propertyAlias, object value)
		{
			var document = new Document(node.Id);

			document.SetProperty(propertyAlias, value);

			return node;
		}

		/// <summary>
		/// Republishes this node
		/// </summary>
		/// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
		/// <param name="useAdminUser">if true then publishes under the context of User(0), if false uses current user</param>
		/// <returns>the same node object on which this is an extension method</returns>
		public static Node Publish(this Node node, bool useAdminUser)
		{
			var document = new Document(node.Id);

			document.Publish(useAdminUser);

			return node;
		}

		/// <summary>
		/// Gets a random node.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <returns>
		/// Returns a random node from a collection of nodes.
		/// </returns>
		public static Node GetRandom(this List<Node> nodes)
		{
			var random = umbraco.library.GetRandom();
			return nodes[random.Next(0, (nodes.Count - 1))];
		}

		/// <summary>
		/// Gets a collection of random nodes.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="numberOfItems">The number of items.</param>
		/// <returns>
		/// Returns the specified number of random nodes from a collection of nodes.
		/// </returns>
		public static IEnumerable<Node> GetRandom(this List<Node> nodes, int numberOfItems)
		{
			var output = new List<Node>(numberOfItems);

			while (output.Count < numberOfItems)
			{
				var randomNode = nodes.GetRandom();
				if (!output.Contains(randomNode))
				{
					output.Add(randomNode);
				}
			}

			return output;
		}

		/// <summary>
		/// Gets the full nice URL.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <returns></returns>
		public static string GetFullNiceUrl(this Node node)
		{
			if (!string.IsNullOrEmpty(node.NiceUrl) && node.NiceUrl[0] == '/')
			{
				return string.Concat(library.RequestServerVariables("HTTP_HOST"), node.NiceUrl);
			}

			return node.NiceUrl;
		}

		/// <summary>
		/// Gets the full nice URL.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="language">The language.</param>
		/// <returns></returns>
		public static string GetFullNiceUrl(this Node node, string language)
		{
			return node.GetFullNiceUrl(language, false);
		}

		/// <summary>
		/// Gets the full nice URL.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="language">The language.</param>
		/// <param name="ssl">if set to <c>true</c> [SSL].</param>
		/// <returns></returns>
		public static string GetFullNiceUrl(this Node node, string language, bool ssl)
		{
			foreach (var domain in library.GetCurrentDomains(node.Id))
			{
				if (domain.Language.Equals(language))
				{
					return node.GetFullNiceUrl(domain, ssl);
				}
			}

			return string.Empty;
		}

		/// <summary>
		/// Gets the full nice URL.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="domain">The domain.</param>
		/// <returns></returns>
		public static string GetFullNiceUrl(this Node node, Domain domain)
		{
			return node.GetFullNiceUrl(domain, false);
		}

		/// <summary>
		/// Gets the full nice URL.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="domain">The domain.</param>
		/// <param name="ssl">if set to <c>true</c> [SSL].</param>
		/// <returns></returns>
		public static string GetFullNiceUrl(this Node node, Domain domain, bool ssl)
		{
			// TODO: [OA] Document on Codeplex
			if (!string.IsNullOrEmpty(node.NiceUrl) && node.NiceUrl[0] == '/')
			{
				return (ssl ? "https://" : "http://") + domain.Name + node.NiceUrl;
			}

			return node.NiceUrl.Replace(library.RequestServerVariables("HTTP_HOST"), string.Concat((ssl ? "https://" : "http://"), domain.Name));
		}

		/// <summary>
		/// Determines whether [the specified node] [is hidden from navigation].
		/// </summary>
		/// <param name="node">The node.</param>
		/// <returns>
		///   <c>true</c> if [the specified node] [is hidden from navigation]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsHiddenFromNavigation(this Node node)
		{
			// TODO: [OA] Document on Codeplex. Is this one really necessary? - [HR] this one could be confusing as depends on default naming convention
			return node.GetProperty<bool>("umbracoNaviHide");
		}

#pragma warning disable 0618
		/// <summary>
		/// Converts legacy nodeFactory collection to INode collection
		/// </summary>
		/// <param name="nodes">The legacy nodeFactory nodes.</param>
		/// <returns>Returns the legacy nodeFactory nodes as INode nodes.</returns>
		public static IEnumerable<Node> ToINodes(this IEnumerable<umbraco.presentation.nodeFactory.Node> nodes)
		{
			return nodes.Select(n => new Node(n.Id));
		}

		/// <summary>
		/// Converts INode collection to legacy nodeFactory collection
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <returns>Returns the nodes as legacy nodeFactory nodes.</returns>
		public static IEnumerable<umbraco.presentation.nodeFactory.Node> ToLegacyNodes(this IEnumerable<Node> nodes)
		{
			return nodes.Select(n => new umbraco.presentation.nodeFactory.Node(n.Id));
		}
#pragma warning restore 0618
	}
}