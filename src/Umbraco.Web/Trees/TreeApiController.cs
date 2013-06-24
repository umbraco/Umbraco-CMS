using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using umbraco.businesslogic;

namespace Umbraco.Web.Trees
{
    //NOTE: We will of course have to authorized this but changing the base class once integrated

    /// <summary>
    /// The base controller for all tree requests
    /// </summary>    
    public abstract class TreeApiController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Remove the xml formatter... only support JSON!
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(global::System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            controllerContext.Configuration.Formatters.Remove(controllerContext.Configuration.Formatters.XmlFormatter);
        }

        protected TreeApiController()
        {           
            //Locate the tree attribute
            var treeAttributes = GetType()
                .GetCustomAttributes(typeof (TreeAttribute), false)
                .OfType<TreeAttribute>()
                .ToArray();
            
            if (treeAttributes.Any() == false)
            {
                throw new InvalidOperationException("The Tree controller is missing the " + typeof(TreeAttribute).FullName + " attribute");
            }

            //assign the properties of this object to those of the metadata attribute
            var attr = treeAttributes.First();
            //TreeId = attr.Id;
            RootNodeDisplayName = attr.Title;
            NodeCollection = new TreeNodeCollection();
        }

        /// <summary>
        /// The method called to render the contents of the tree structure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings">
        /// All of the query string parameters passed from jsTree
        /// </param>
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be pased in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        protected abstract TreeNodeCollection GetTreeData(string id, FormDataCollection queryStrings);

        ///// <summary>
        ///// Returns the root node for the tree
        ///// </summary>
        //protected abstract string RootNodeId { get; }
        
        /// <summary>
        /// The name to display on the root node
        /// </summary>
        public string RootNodeDisplayName { get; private set; }
        
        /// <summary>
        /// The action called to render the contents of the tree structure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings">
        /// All of the query string parameters passed from jsTree
        /// </param>
        /// <returns>JSON markup for jsTree</returns>        
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be pased in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        [HttpQueryStringFilter("queryStrings")]
        public TreeNodeCollection GetNodes(string id, FormDataCollection queryStrings)
        {
            if (queryStrings == null) queryStrings = new FormDataCollection("");
            //if its the root node, render it otherwise render normal nodes
            return AddRootNodeToCollection(id, queryStrings)
                       ? NodeCollection
                       : GetTreeData(id, queryStrings);
        }

        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected virtual TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var getChildNodesUrl = Url.GetTreeUrl(
                GetType(), 
                Constants.System.Root.ToString(CultureInfo.InvariantCulture), 
                queryStrings);

            var isDialog = queryStrings.GetValue<bool>(TreeQueryStringParameters.DialogMode);
            //var node = new TreeNode(RootNodeId, BackOfficeRequestContext.RegisteredComponents.MenuItems, jsonUrl)
            var node = new TreeNode(Constants.System.Root.ToString(CultureInfo.InvariantCulture), getChildNodesUrl)
                {
                    HasChildren = true,

                    ////THIS IS TEMPORARY UNTIL WE FIGURE OUT HOW WE ARE LOADING STUFF (I.E. VIEW NAMES, ACTION NAMES, DUNNO)
                    //EditorUrl = queryStrings.HasKey(TreeQueryStringParameters.OnNodeClick) //has a node click handler?
                    //                ? queryStrings.Get(TreeQueryStringParameters.OnNodeClick) //return node click handler
                    //                : isDialog //is in dialog mode without a click handler ?
                    //                      ? "#" //return empty string, otherwise, return an editor URL:
                    //                      : "mydashboard", 

                    Title = RootNodeDisplayName
                };

            //add the tree type to the root
            node.AdditionalData.Add("treeType", GetType().FullName);
            
            ////add the tree-root css class
            //node.Style.AddCustom("tree-root");

            //node.AdditionalData.Add("id", node.HiveId.ToString());
            //node.AdditionalData.Add("title", node.Title);

            AddQueryStringsToAdditionalData(node, queryStrings);

            //check if the tree is searchable and add that to the meta data as well
            if (this is ISearchableTree)
            {
                node.AdditionalData.Add("searchable", "true");
            }

            return node;
        }

        /// <summary>
        /// The AdditionalData of a node is always populated with the query string data, this method performs this
        /// operation and ensures that special values are not inserted or that duplicate keys are not added.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="queryStrings"></param>
        protected virtual void AddQueryStringsToAdditionalData(TreeNode node, FormDataCollection queryStrings)
        {
            // Add additional data, ensure treeId isn't added as we've already done that
            foreach (var q in queryStrings
                .Where(x => x.Key != "treeId" && node.AdditionalData.ContainsKey(x.Key) == false))
            {
                node.AdditionalData.Add(q.Key, q.Value);
            }
        }

        /// <summary>
        /// Checks if the node Id is the root node and if so creates the root node and appends it to the 
        /// NodeCollection based on the standard tree parameters
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method ensure that all of the correct meta data is set for the root node so that the Umbraco application works
        /// as expected. Meta data such as 'treeId' and 'searchable'
        /// </remarks>
        protected bool AddRootNodeToCollection(string id, FormDataCollection queryStrings)
        {                       
            //if its the root model
            if (id == Constants.System.Root.ToString(CultureInfo.InvariantCulture))
            {
                //get the root model
                var rootNode = CreateRootNode(queryStrings);
             
                NodeCollection.Add(rootNode);

                return true;
            }

            return false;
        }

        #region Create TreeNode methods

        ///// <summary>
        ///// Helper method to create tree nodes and automatically generate the json url
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="queryStrings"></param>
        ///// <param name="title"></param>
        ///// <param name="editorUrl"></param>
        ///// <returns></returns>
        //public TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, string title, string editorUrl)
        //{
        //    var jsonUrl = Url.GetTreeUrl(GetType(), id, queryStrings);
        //    //var node = new TreeNode(id, BackOfficeRequestContext.RegisteredComponents.MenuItems, jsonUrl) { Title = title, EditorUrl = editorUrl };
        //    var node = new TreeNode(id, jsonUrl) { Title = title, EditorUrl = editorUrl };
        //    return node;
        //}

        ///// <summary>
        ///// Helper method to create tree nodes and automatically generate the json url
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="queryStrings"></param>
        ///// <param name="title"></param>
        ///// <param name="editorUrl"></param>
        ///// <param name="action"></param>
        ///// <returns></returns>
        //public TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, string title, string editorUrl, string action)
        //{
        //    var jsonUrl = Url.GetTreeUrl(GetType(), id, queryStrings);
        //    //var node = new TreeNode(id, BackOfficeRequestContext.RegisteredComponents.MenuItems, jsonUrl) { Title = title, EditorUrl = editorUrl };
        //    var node = new TreeNode(id, jsonUrl) { Title = title, EditorUrl = editorUrl };
        //    return node;
        //}

        ///// <summary>
        ///// Helper method to create tree nodes and automatically generate the json url
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="queryStrings"></param>
        ///// <param name="title"></param>
        ///// <param name="editorUrl"></param>
        ///// <param name="action"></param>
        ///// <param name="icon"></param>
        ///// <returns></returns>
        //public TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, string title, string editorUrl, string action, string icon)
        //{
        //    var jsonUrl = Url.GetTreeUrl(GetType(), id, queryStrings);
        //    //var node = new TreeNode(id, BackOfficeRequestContext.RegisteredComponents.MenuItems, jsonUrl) { Title = title, EditorUrl = editorUrl };
        //    var node = new TreeNode(id, jsonUrl) { Title = title, EditorUrl = editorUrl };
        //    return node;
        //}

        ///// <summary>
        ///// Helper method to create tree nodes and automatically generate the json url
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="queryStrings"></param>
        ///// <param name="title"></param>
        ///// <param name="editorUrl"></param>
        ///// <param name="action"></param>
        ///// <param name="hasChildren"></param>
        ///// <returns></returns>
        //public TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, string title, string editorUrl, string action, bool hasChildren)
        //{
        //    var treeNode = CreateTreeNode(id, queryStrings, title, editorUrl, action);
        //    treeNode.HasChildren = hasChildren;
        //    return treeNode;
        //}

        ///// <summary>
        ///// Helper method to create tree nodes and automatically generate the json url
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="queryStrings"></param>
        ///// <param name="title"></param>
        ///// <param name="editorUrl"></param>
        ///// <param name="action"></param>
        ///// <param name="hasChildren"></param>
        ///// <param name="icon"></param>
        ///// <returns></returns>
        //public TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, string title, string editorUrl, string action, bool hasChildren, string icon)
        //{
        //    var treeNode = CreateTreeNode(id, queryStrings, title, editorUrl, action);
        //    treeNode.HasChildren = hasChildren;
        //    treeNode.Icon = icon;
        //    return treeNode;
        //}

        ///// <summary>
        ///// Helper method to create tree nodes and automatically generate the json url
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="queryStrings"></param>
        ///// <param name="title"></param>
        ///// <param name="editorUrl"></param>
        ///// <param name="hasChildren"></param>
        ///// <returns></returns>
        //public TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, string title, string editorUrl, bool hasChildren)
        //{
        //    var treeNode = CreateTreeNode(id, queryStrings, title, editorUrl);
        //    treeNode.HasChildren = hasChildren;
        //    return treeNode;
        //}

        ///// <summary>
        ///// Helper method to create tree nodes and automatically generate the json url
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="queryStrings"></param>
        ///// <param name="title"></param>
        ///// <param name="editorUrl"></param>
        ///// <param name="hasChildren"></param>
        ///// <param name="icon"></param>
        ///// <returns></returns>
        //public TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, string title, string editorUrl, bool hasChildren, string icon)
        //{
        //    var treeNode = CreateTreeNode(id, queryStrings, title, editorUrl);
        //    treeNode.HasChildren = hasChildren;
        //    treeNode.Icon = icon;
        //    return treeNode;
        //} 

        #endregion

        /// <summary>
        /// The tree name based on the controller type so that everything is based on naming conventions
        /// </summary>
        public string TreeType
        {
            get
            {
                var name = GetType().Name;
                return name.Substring(0, name.LastIndexOf("TreeController", StringComparison.Ordinal));
            }
        }
        
        /// <summary>
        /// The model collection for trees to add nodes to
        /// </summary>
        protected TreeNodeCollection NodeCollection { get; private set; }
        
    }
}
