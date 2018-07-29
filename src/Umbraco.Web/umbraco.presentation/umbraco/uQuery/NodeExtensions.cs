using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
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
            return (Node)GetAncestorByPathLevel((INode)node, level);
        }

        /// <summary>
        /// Gets the ancestor by path level.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="level">The level.</param>
        /// <returns>Returns an ancestor node by path level.</returns>
        public static INode GetAncestorByPathLevel(this INode node, int level)
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
            return GetAncestorNodes((INode)node).Cast<Node>();
        }

        /// <summary>
        /// Functionally similar to the XPath axis 'ancestor'
        /// Get the Ancestor Nodes from current to root (useful for breadcrumbs)
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <returns>INode as IEnumerable</returns>
        public static IEnumerable<INode> GetAncestorNodes(this INode node)
        {
            var ancestor = node.Parent;

            while (ancestor != null)
            {
                yield return ancestor as INode;

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
            return GetAncestorOrSelfNodes((INode)node).Cast<Node>();
        }

        /// <summary>
        /// Functionally similar to the XPath axis 'ancestor-or-self'
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <returns>INode as IEnumerable</returns>
        public static IEnumerable<INode> GetAncestorOrSelfNodes(this INode node)
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
            return GetPrecedingSiblingNodes((INode)node).Cast<Node>();
        }

        /// <summary>
        /// Functionally similar to the XPath axis 'preceding-sibling'
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <returns>INode as IEumerable</returns>
        public static IEnumerable<INode> GetPrecedingSiblingNodes(this INode node)
        {
            if (node.Parent != null)
            {
                var parentNode = node.Parent as INode;
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
            return GetFollowingSiblingNodes((INode)node).Cast<Node>();
        }

        /// <summary>
        /// Functionally similar to the XPath axis 'following-sibling'
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <returns>INode as IEumerable</returns>
        public static IEnumerable<INode> GetFollowingSiblingNodes(this INode node)
        {
            if (node.Parent != null)
            {
                var parentNode = node.Parent as INode;
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
            return GetSiblingNodes((INode)node).Cast<Node>();
        }

        /// <summary>
        /// Gets all sibling Nodes
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <returns>INode as IEumerable</returns>
        public static IEnumerable<INode> GetSiblingNodes(this INode node)
        {
            if (node.Parent != null)
            {
                var parentNode = node.Parent as INode;
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
            return GetDescendantOrSelfNodes((INode)node).Cast<Node>();
        }

        /// <summary>
        /// Functionally similar to the XPath axis 'descendant-or-self'
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <returns>INode as IEnumerable</returns>
        public static IEnumerable<INode> GetDescendantOrSelfNodes(this INode node)
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
        /// taken from: https://our.umbraco.com/wiki/how-tos/useful-helper-extension-methods-%28linq-null-safe-access%29
        /// </summary>
        /// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
        /// <returns>Node as IEnumerable</returns>
        public static IEnumerable<Node> GetDescendantNodes(this Node node)
        {
            return GetDescendantNodes((INode)node).Cast<Node>();
        }

        /// <summary>
        /// Functionally similar to the XPath axis 'descendant'
        /// Make the All Descendants LINQ queryable
        /// taken from: https://our.umbraco.com/wiki/how-tos/useful-helper-extension-methods-%28linq-null-safe-access%29
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <returns>INode as IEnumerable</returns>
        public static IEnumerable<INode> GetDescendantNodes(this INode node)
        {
            foreach (INode child in node.ChildrenAsList)
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
            Func<INode, bool> convertedFunc = x => func((Node)x);
            return GetDescendantNodes((INode)node, convertedFunc).Cast<Node>();
        }

        /// <summary>
        /// Drills down into the descendant nodes returning those where Func is true, when Func is false further descendants are not checked
        /// taken from: http://ucomponents.codeplex.com/discussions/246406
        /// </summary>
        /// <param name="node">The <c>umbraco.interfaces.INode</c>.</param>
        /// <param name="func">The func</param>
        /// <returns>INode as IEnumerable</returns>
        public static IEnumerable<INode> GetDescendantNodes(this INode node, Func<INode, bool> func)
        {
            foreach (INode child in node.ChildrenAsList)
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
        /// Get all descendants, and then return only those that match the requested typeAlias
        /// </summary>
        /// <param name="node">The <c>umbraco.presentation.nodeFactory.Node</c>.</param>
        /// <param name="documentTypeAlias">The document type alias.</param>
        /// <returns>Nodes as IEnumerable</returns>
        public static IEnumerable<Node> GetDescendantNodesByType(this Node node, string documentTypeAlias)
        {
            return GetDescendantNodesByType((INode)node, documentTypeAlias).Cast<Node>();
        }

        /// <summary>
        /// Gets the descendant nodes by document-type.
        /// Get all descendants, and then return only those that match the requested typeAlias
        /// </summary>
        /// <param name="node">The <c>umbraco.interfaces.INode</c>.</param>
        /// <param name="documentTypeAlias">The document type alias.</param>
        /// <returns>INodes as IEnumerable</returns>
        public static IEnumerable<INode> GetDescendantNodesByType(this INode node, string documentTypeAlias)
        {
            return node.GetDescendantNodes().Where(x => x.NodeTypeAlias == documentTypeAlias);
        }

        /// <summary>
        /// Functionally similar to the XPath axis 'child'
        /// Make the imediate Children LINQ queryable
        /// Performance optimised for just imediate children.
        /// taken from: https://our.umbraco.com/wiki/how-tos/useful-helper-extension-methods-%28linq-null-safe-access%29
        /// </summary>
        /// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
        /// <returns>Node as IEnumerable</returns>
        public static IEnumerable<Node> GetChildNodes(this Node node)
        {
            return GetChildNodes((INode)node).Cast<Node>();
        }

        /// <summary>
        /// Functionally similar to the XPath axis 'child'
        /// Make the imediate Children LINQ queryable
        /// Performance optimised for just imediate children.
        /// taken from: https://our.umbraco.com/wiki/how-tos/useful-helper-extension-methods-%28linq-null-safe-access%29
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <returns>INode as IEnumerable</returns>
        public static IEnumerable<INode> GetChildNodes(this INode node)
        {
            return node.ChildrenAsList;
        }

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        /// <param name="node">The <c>umbraco.presentation.nodeFactory.Node</c>.</param>
        /// <param name="func">The func.</param>
        /// <returns>Nodes as IEnumerable</returns>
        public static IEnumerable<Node> GetChildNodes(this Node node, Func<Node, bool> func)
        {
            Func<INode, bool> convertedFunc = x => func((Node)x);
            return GetChildNodes((INode)node, convertedFunc).Cast<Node>();
        }

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        /// <param name="node">The <c>umbraco.interfaces.INode</c>.</param>
        /// <param name="func">The func.</param>
        /// <returns>INodes as IEnumerable</returns>
        public static IEnumerable<INode> GetChildNodes(this INode node, Func<INode, bool> func)
        {
            foreach (INode child in node.ChildrenAsList)
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
            return GetChildNodesByType((INode)node, documentTypeAlias).Cast<Node>();
        }

        /// <summary>
        /// Gets the child nodes by document-type.
        /// </summary>
        /// <param name="node">The <c>umbraco.interfaces.INode</c>.</param>
        /// <param name="documentTypeAlias">The document type alias.</param>
        /// <returns>INodes as IEnumerable</returns>
        public static IEnumerable<INode> GetChildNodesByType(this INode node, string documentTypeAlias)
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
            return (Node)GetChildNodeByName((INode)parentNode, nodeName);
        }

        /// <summary>
        /// Extension method on Node to retun a matching child node by name
        /// </summary>
        /// <param name="parentNode">an umbraco.interfaces.INode object</param>
        /// <param name="nodeName">name of node to search for</param>
        /// <returns>null or INode</returns>
        public static INode GetChildNodeByName(this INode parentNode, string nodeName)
        {
            return parentNode.ChildrenAsList.FirstOrDefault(child => child.Name == nodeName);
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
            return HasProperty((INode)node, propertyAlias);
        }

        /// <summary>
        /// Determines whether the specified node has property.
        /// </summary>
        /// <param name="node">The INode.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns>
        ///   <c>true</c> if the specified node has property; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasProperty(this INode node, string propertyAlias)
        {
            var property = node.GetProperty(propertyAlias);
            return (property != null);
        }

        /// <summary>
        /// Get a value of type T from a property
        /// </summary>
        /// <typeparam name="T">type T to cast to</typeparam>
        /// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
        /// <param name="propertyAlias">alias of property to get</param>
        /// <returns>default(T) or property cast to (T)</returns>
        public static T GetProperty<T>(this Node node, string propertyAlias)
        {
            return GetProperty<T>((INode)node, propertyAlias);
        }

        /// <summary>
        /// Get a value of type T from a property
        /// </summary>
        /// <typeparam name="T">type T to cast to</typeparam>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <param name="propertyAlias">alias of property to get</param>
        /// <returns>default(T) or property cast to (T)</returns>
        public static T GetProperty<T>(this INode node, string propertyAlias)
        {
            // check to see if return object handles it's own object hydration
            if (typeof(uQuery.IGetProperty).IsAssignableFrom(typeof(T)))
            {
                // create new instance of T with empty constructor
                uQuery.IGetProperty t = (uQuery.IGetProperty)Activator.CreateInstance<T>();

                // call method to hydrate the object from a string value
                t.LoadPropertyValue(node.GetProperty<string>(propertyAlias));

                return (T)t;
            }

            //TODO: This should be converted to our extension method TryConvertTo<T> ....

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (typeConverter != null)
            {

                // Boolean
                if (typeof(T) == typeof(bool))
                {
                    // Use the GetPropertyAsBoolean method, as this handles true also being stored as "1"
                    return (T)typeConverter.ConvertFrom(node.GetPropertyAsBoolean(propertyAlias).ToString());
                }

                // XmlDocument
                else if (typeof(T) == typeof(XmlDocument))
                {
                    var xmlDocument = new XmlDocument();

                    try
                    {
                        xmlDocument.LoadXml(node.GetPropertyAsString(propertyAlias));
                    }
                    catch
                    {
                        // xml probably invalid
                    }

                    return (T)((object)xmlDocument);
                }

//                // umbraco.MacroEngines DynamicXml
                //                else if (typeof(T) == typeof(DynamicXml))
                //                {
                //                    try
                //                    {
                //                        return (T)((object)new DynamicXml(node.GetPropertyAsString(propertyAlias)));
                //                    }
                //                    catch
                //                    {
                //                    }
                //                }
                else
                {
                    try
                    {
                        return (T)typeConverter.ConvertFromString(node.GetPropertyAsString(propertyAlias));
                    }
                    catch
                    {
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// Get a string value for the supplied property alias
        /// </summary>
        /// <param name="node">an umbraco.presentation.nodeFactory.Node object</param>
        /// <param name="propertyAlias">alias of propety to get</param>
        /// <returns>empty string, or property value as string</returns>
        private static string GetPropertyAsString(this Node node, string propertyAlias)
        {
            return GetPropertyAsString((INode)node, propertyAlias);
        }

        /// <summary>
        /// Get a string value for the supplied property alias
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <param name="propertyAlias">alias of propety to get</param>
        /// <returns>empty string, or property value as string</returns>
        private static string GetPropertyAsString(this INode node, string propertyAlias)
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
        private static bool GetPropertyAsBoolean(this Node node, string propertyAlias)
        {
            return GetPropertyAsBoolean((INode)node, propertyAlias);
        }

        /// <summary>
        /// Get a boolean value for the supplied property alias (works with built in Yes/No dataype)
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <param name="propertyAlias">alias of propety to get</param>
        /// <returns>true if can cast value, else false for all other circumstances</returns>
        private static bool GetPropertyAsBoolean(this INode node, string propertyAlias)
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
        /// Tell me the level of this node (0 = root)
        /// updated from Depth and changed to start at 0
        /// to align with other 'Level' methods (eg xslt)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static int Level(this Node node)
        {
            return Level((INode)node);
        }

        /// <summary>
        /// Tell me the level of this node (0 = root)
        /// updated from Depth and changed to start at 0
        /// to align with other 'Level' methods (eg xslt)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static int Level(this INode node)
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
            return ToXml((INode)node);
        }

        /// <summary>
        /// Gets the XML for the Node.
        /// </summary>
        /// <param name="node">The INode.</param>
        /// <returns>Returns an <c>XmlNode</c> for the selected Node</returns>
        public static XmlNode ToXml(this INode node)
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
            return GetImageCropperUrl((INode)node, propertyAlias, cropName);
        }

        /// <summary>
        /// Returns the url for a given crop name using the built in Image Cropper datatype
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <param name="propertyAlias">property alias</param>
        /// <param name="cropName">name of crop to get url for</param>
        /// <returns>emtpy string or url</returns>
        public static string GetImageCropperUrl(this INode node, string propertyAlias, string cropName)
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
            return (Node)SetProperty((INode)node, propertyAlias, value);
        }

        /// <summary>
        /// Sets a property value on this node
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <param name="propertyAlias">alias of property to set</param>
        /// <param name="value">value to set</param>
        /// <returns>the same node object on which this is an extension method</returns>
        public static INode SetProperty(this INode node, string propertyAlias, object value)
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
            return (Node)Publish((INode)node, useAdminUser);
        }

        /// <summary>
        /// Republishes this node
        /// </summary>
        /// <param name="node">an umbraco.interfaces.INode object</param>
        /// <param name="useAdminUser">if true then publishes under the context of User(0), if false uses current user</param>
        /// <returns>the same node object on which this is an extension method</returns>
        public static INode Publish(this INode node, bool useAdminUser)
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
        public static Node GetRandom(this IList<Node> nodes)
        {
            return (Node)GetRandom(nodes.Cast<INode>().ToList());
        }

        /// <summary>
        /// Gets a random node.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns>
        /// Returns a random INode from a collection of INodes.
        /// </returns>
        public static INode GetRandom(this IList<INode> nodes)
        {
            var random = umbraco.library.GetRandom();
            return nodes[random.Next(0, nodes.Count)];
        }

        /// <summary>
        /// Gets a collection of random nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="numberOfItems">The number of items.</param>
        /// <returns>
        /// Returns the specified number of random nodes from a collection of nodes.
        /// </returns>
        public static IEnumerable<Node> GetRandom(this IList<Node> nodes, int numberOfItems)
        {
            var randomNodes = GetRandom(nodes.Cast<INode>().ToList(), numberOfItems);
            return randomNodes.Cast<Node>().ToList();
        }

        /// <summary>
        /// Gets a collection of random nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="numberOfItems">The number of items.</param>
        /// <returns>
        /// Returns the specified number of random INodes from a collection of INodes.
        /// </returns>
        public static IEnumerable<INode> GetRandom(this IList<INode> nodes, int numberOfItems)
        {
            var output = new List<INode>(numberOfItems);

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
            return GetFullNiceUrl((INode)node);
        }

        /// <summary>
        /// Gets the full nice URL.
        /// </summary>
        /// <param name="node">The node as INode.</param>
        /// <returns></returns>
        public static string GetFullNiceUrl(this INode node)
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
            return GetFullNiceUrl((INode)node, language);
        }

        /// <summary>
        /// Gets the full nice URL.
        /// </summary>
        /// <param name="node">The node as INode.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public static string GetFullNiceUrl(this INode node, string language)
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
            return GetFullNiceUrl((INode)node, language, ssl);
        }

        /// <summary>
        /// Gets the full nice URL.
        /// </summary>
        /// <param name="node">The node as INode.</param>
        /// <param name="language">The language.</param>
        /// <param name="ssl">if set to <c>true</c> [SSL].</param>
        /// <returns></returns>
        public static string GetFullNiceUrl(this INode node, string language, bool ssl)
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
            return GetFullNiceUrl((INode)node, domain);
        }

        /// <summary>
        /// Gets the full nice URL.
        /// </summary>
        /// <param name="node">The node as INode.</param>
        /// <param name="domain">The domain.</param>
        /// <returns></returns>
        public static string GetFullNiceUrl(this INode node, Domain domain)
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
            return GetFullNiceUrl((INode)node, domain, ssl);
        }

        /// <summary>
        /// Gets the full nice URL.
        /// </summary>
        /// <param name="node">The node as INode.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="ssl">if set to <c>true</c> [SSL].</param>
        /// <returns></returns>
        public static string GetFullNiceUrl(this INode node, Domain domain, bool ssl)
        {
            // TODO: [OA] Document on Codeplex
            if (!string.IsNullOrEmpty(node.NiceUrl) && node.NiceUrl[0] == '/')
            {
                return (ssl ? "https://" : "http://") + domain.Name + node.NiceUrl;
            }

            return node.NiceUrl.Replace(library.RequestServerVariables("HTTP_HOST"), string.Concat((ssl ? "https://" : "http://"), domain.Name));
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