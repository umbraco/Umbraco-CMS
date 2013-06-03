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
        //private readonly IEnumerable<Lazy<MenuItem, MenuItemMetadata>> _menuItems;

        //public TreeNode(string nodeId, IEnumerable<Lazy<MenuItem, MenuItemMetadata>> menuItems, string jsonUrl)
        public TreeNode(string nodeId, string getChildNodesUrl)
        {
            //_menuItems = menuItems;
            //Style = new NodeStyle();
            //MenuActions = new List<Lazy<MenuItem, MenuItemMetadata>>();
            NodeId = nodeId;
            AdditionalData = new Dictionary<string, object>();
            ChildNodesUrl = getChildNodesUrl;
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
        [DataMember(Name = "nodeId")]
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
        
        ///// <summary>
        ///// Gets or sets the node path.
        ///// </summary>
        ///// <value>
        ///// The node path.
        ///// </value>
        //public string NodePath { get; private set; }

        /// <summary>
        /// The icon to use for the node, this can be either a path to an image or a Css class. 
        /// If a '/' is found in the string then it will be considered a path to an image.
        /// </summary>
        [DataMember(Name = "icon")]
        public string Icon { get; set; }

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

        /// <summary>
        /// The URL path for the editor for this model
        /// </summary>
        [DataMember(Name = "editorUrl")]
        public string EditorUrl { get; set; }

        /// <summary>
        /// The JSON url to load the nodes children
        /// </summary>
        [DataMember(Name = "childNodesUrl")]
        public string ChildNodesUrl { get; set; }

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

        ///// <summary>
        ///// A collection of context menu actions to apply for the model
        ///// </summary>
        //internal List<Lazy<MenuItem, MenuItemMetadata>> MenuActions { get; private set; }

        ///// <summary>
        ///// Adds a menu item
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        //public void AddMenuItem<T>()
        //   where T : MenuItem
        //{
        //    AddMenuItem<T>(null);
        //}

        ///// <summary>
        ///// Adds a menu item with a key value pair which is merged to the AdditionalData bag
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        //public void AddMenuItem<T>(string key, string value)
        //   where T : MenuItem
        //{
        //    AddMenuItem<T>(new Dictionary<string, object> { { key, value } });
        //}
       
        ///// <summary>
        ///// Adds a menu item with a dictionary which is merged to the AdditionalData bag
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="additionalData"></param>
        //public void AddMenuItem<T>(IDictionary<string, object> additionalData)
        //   where T : MenuItem
        //{
        //    var item = _menuItems.GetItem<T>();
        //    if (item != null)
        //    {
        //        MenuActions.Add(item);
        //        if (additionalData != null)
        //        {
        //            //merge the additional data!
        //            AdditionalData = AdditionalData.MergeLeft(additionalData);                   
        //        }

        //        //validate the data in the meta data bag
        //        item.Value.ValidateRequiredData(AdditionalData);
        //    }
        //}

    }
}
