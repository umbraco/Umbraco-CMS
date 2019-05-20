using System.Runtime.Serialization;
using Umbraco.Core.IO;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Exceptions;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// Represents a model in the tree
    /// </summary>
    /// <remarks>
    /// TreeNode is sealed to prevent developers from adding additional json data to the response
    /// </remarks>
    [DataContract(Name = "node", Namespace = "")]
    public class TreeNode : EntityBasic
    {
        /// <summary>
        /// Internal constructor, to create a tree node use the CreateTreeNode methods of the TreeApiController.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="parentId">The parent id for the current node</param>
        /// <param name="getChildNodesUrl"></param>
        /// <param name="menuUrl"></param>
        internal TreeNode(string nodeId, string parentId, string getChildNodesUrl, string menuUrl)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) throw new ArgumentNullOrEmptyException(nameof(nodeId));

            Id = nodeId;
            ParentId = parentId;
            ChildNodesUrl = getChildNodesUrl;
            MenuUrl = menuUrl;
            CssClasses = new List<string>();
             //default
            Icon = "icon-folder-close";
            Path = "-1";
        }

        [DataMember(Name = "parentId", IsRequired = true)]
        public new object ParentId { get; set; }

        /// <summary>
        /// A flag to set whether or not this node has children
        /// </summary>
        [DataMember(Name = "hasChildren")]
        public bool HasChildren { get; set; }

        /// <summary>
        /// The tree nodetype which refers to the type of node rendered in the tree
        /// </summary>
        [DataMember(Name = "nodeType")]
        public string NodeType { get; set; }

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
        /// A list of additional/custom css classes to assign to the node
        /// </summary>
        [DataMember(Name = "cssClasses")]
        public IList<string> CssClasses { get; private set; }
    }

}
