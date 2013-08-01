using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using Umbraco.Core;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Represents a model in the tree
    /// </summary>
    /// <remarks>
    /// TreeNode is sealed to prevent developers from adding additional json data to the response
    /// </remarks>
    [DataContract(Name = "node", Namespace = "")]
    public class TreeNode 
    {
        /// <summary>
        /// Internal constructor, to create a tree node use the CreateTreeNode methods of the TreeApiController.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="getChildNodesUrl"></param>
        /// <param name="menuUrl"></param>
        internal TreeNode(string nodeId, string getChildNodesUrl, string menuUrl)
        {
            //_menuItems = menuItems;
            //Style = new NodeStyle();
            NodeId = nodeId;
            AdditionalData = new Dictionary<string, object>();
            ChildNodesUrl = getChildNodesUrl;
            MenuUrl = menuUrl;
            //default
            Icon = "icon-folder-close";
        }

        /// <summary>
        /// The unique identifier for the node
        /// </summary>
        [DataMember(Name = "id")]
        public string NodeId { get; private set; }

        /// <summary>
        /// A flag to set whether or not this node has children
        /// </summary>
        [DataMember(Name = "hasChildren")]
        public bool HasChildren { get; set; }

        /// <summary>
        /// The text title of the node that is displayed in the tree
        /// </summary>
        [DataMember(Name = "name")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the node path.
        /// </summary>
        [DataMember(Name = "nodePath")]
        public string NodePath { get; set; }

        /// <summary>
        /// The icon to use for the node, this can be either a path to an image or a Css class. 
        /// If a '/' is found in the string then it will be considered a path to an image.
        /// </summary>
        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        /// <summary>
        /// The tree nodetype which refers to the type of node rendered in the tree
        /// </summary>
        [DataMember(Name = "nodetype")]
        public string NodeType { get; set; }

        /// <summary>
        /// Returns true if the icon represents a CSS class instead of a file path
        /// </summary>
        [DataMember(Name = "iconIsClass")]
        public bool IconIsClass
        {
            get
            {
                if (Icon.IsNullOrWhiteSpace())
                {
                    return true;
                }
                //if it starts with a '.' or doesn't contain a '.' at all then it is a class
                return Icon.StartsWith(".") || Icon.Contains(".") == false;
            }
        }

        /// <summary>
        /// Returns the icon file path if the icon is not a class, otherwise returns an empty string
        /// </summary>
        [DataMember(Name = "iconFilePath")]
        public string IconFilePath
        {
            get
            {
                return IconIsClass
                           ? string.Empty
                           : IOHelper.ResolveUrl("~/umbraco/images/umbraco/" + Icon);
            }
        }

        /// <summary>
        /// Optional: The Route path for the editor for this node
        /// </summary>
        /// <remarks>
        /// If this is not set, then the route path will be automatically determined by: {section}/edit/{id}
        /// </remarks>
        [DataMember(Name = "routePath")]
        public string RoutePath { get; set; }

        /// <summary>
        /// The JSON url to load the nodes children
        /// </summary>
        [DataMember(Name = "childNodesUrl")]
        public string ChildNodesUrl { get; set; }

        /// <summary>
        /// The JSON url to load the menu from
        /// </summary>
        [DataMember(Name = "menuUrl")]
        public string MenuUrl { get; set; }

        /// <summary>
        /// A dictionary to support any additional meta data that should be rendered for the node which is 
        /// useful for custom action commands such as 'create', 'copy', etc...
        /// </summary>
        [DataMember(Name = "metaData")]
        public Dictionary<string, object> AdditionalData { get; private set; }
        
        ///// <summary>
        ///// The UI style to give the model
        ///// </summary>
        //public NodeStyle Style { get; private set; }
    }
}
