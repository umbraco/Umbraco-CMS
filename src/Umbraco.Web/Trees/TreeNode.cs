using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.IO;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Represents a model in the tree
    /// </summary>
    [DataContract(Name = "node", Namespace = "")]
    public class TreeNode
    {
        //private readonly List<MenuItem> _menuItems = new List<MenuItem>();

        public TreeNode(string nodeId, string getChildNodesUrl, string menuUrl)
        {
            //_menuItems = menuItems;
            //Style = new NodeStyle();
            NodeId = nodeId;
            AdditionalData = new Dictionary<string, object>();
            ChildNodesUrl = getChildNodesUrl;
            MenuUrl = menuUrl;
        }

        /// <summary>
        /// Internal child array to be able to add nested levels of content
        /// </summary>
        /// <remarks>
        /// Currently this is ONLY supported for rendering the root node for an application
        /// tree when there are multiple trees for an application
        /// </remarks>
        internal IEnumerable<TreeNode> Children { get; set; }

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
        [DataMember(Name = "path")]
        public string Path { get; set; }

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

        ///// <summary>
        ///// The URL path for the editor for this model
        ///// </summary>
        ///// <remarks>
        ///// If this is set then the UI will attempt to load the result of this URL into an IFrame
        ///// for the editor.
        ///// 
        ///// Generally for use with Legacy trees
        ///// </remarks>
        //[DataMember(Name = "editorUrl")]
        //internal string EditorUrl { get; set; }

        /// <summary>
        /// A JS method/codeblock to be called when the tree node is clicked
        /// </summary>
        /// <remarks>
        /// This should only be used for legacy trees
        /// </remarks>
        [DataMember(Name = "jsClickCallback")]
        internal string OnClickCallback { get; set; }

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

        ///// <summary>
        ///// The UI style to give the model
        ///// </summary>
        //public NodeStyle Style { get; private set; }

        /// <summary>
        /// A dictionary to support any additional meta data that should be rendered for the node which is 
        /// useful for custom action commands such as 'create', 'copy', etc...
        /// </summary>
        [DataMember(Name = "metaData")]
        public Dictionary<string, object> AdditionalData { get; private set; }

    }
}
