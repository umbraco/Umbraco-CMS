namespace Umbraco.Web.Trees
{
    using System.Net.Http.Formatting;

    using global::umbraco;

    using Umbraco.Core;
    using Umbraco.Web.Models.Trees;
    using Umbraco.Web.WebApi.Filters;

    [UmbracoTreeAuthorize(Constants.Trees.Dictionary)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Dictionary, null, sortOrder: 3)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    [LegacyBaseTree(typeof(loadDictionary))]
    public class DictionaryTreeController : TreeController
    {
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
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            return nodes;
        }

        /// <summary>
        /// Returns the menu structure for the node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            return menu;
        }
    }
}
