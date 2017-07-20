namespace Umbraco.Web.Trees
{
    using System;
    using System.Linq;
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
            var intId = id.TryConvertTo<int>();
            if (intId == false)
            {
                throw new InvalidOperationException("Id must be an integer");
            }

            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                nodes.AddRange(
                    this.Services.LocalizationService.GetRootDictionaryItems().Select(
                        x => this.CreateTreeNode(
                            x.Id.ToInvariantString(),
                            id,
                            queryStrings,
                            x.ItemKey,
                            "icon-book-alt",
                            true)));
            }
            else
            {
                // maybe we should use the guid as url param to avoid the extra call for getting dictionary item
                var parentDictionary = this.Services.LocalizationService.GetDictionaryItemById(intId.Result);

                if (parentDictionary == null)
                {
                    return nodes;
                }

                nodes.AddRange(this.Services.LocalizationService.GetDictionaryItemChildren(parentDictionary.Key).Select(
                    x => this.CreateTreeNode(
                        x.Id.ToInvariantString(),
                        id,
                        queryStrings,
                        x.ItemKey,
                        "icon-book-alt", true)));
            }

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
