using System;
using System.Linq;
using System.Net.Http.Formatting;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Dictionary)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    [Tree(Constants.Applications.Settings, Constants.Trees.Dictionary, null, sortOrder: 3)]
    public class DictionaryTreeController : TreeController
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            // the default section is settings, falling back to this if we can't
            // figure out where we are from the querystring parameters
            var section = Constants.Applications.Settings;
            if (queryStrings["application"] != null)
                section = queryStrings["application"];

            // this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{section}/{Constants.Trees.Dictionary}/list";

            return root;
        }

        /// <summary>
        /// The method called to render the contents of the tree structure
        /// </summary>
        /// <param name="id">The id of the tree item</param>
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
                throw new InvalidOperationException("Id must be an integer");

            var nodes = new TreeNodeCollection();

            Func<IDictionaryItem, string> ItemSort() => item => item.ItemKey;

            if (id == Constants.System.Root.ToInvariantString())
            {
                nodes.AddRange(
                    Services.LocalizationService.GetRootDictionaryItems().OrderBy(ItemSort()).Select(
                        x => CreateTreeNode(
                            x.Id.ToInvariantString(),
                            id,
                            queryStrings,
                            x.ItemKey,
                            "icon-book-alt",
                            Services.LocalizationService.GetDictionaryItemChildren(x.Key).Any())));
            }
            else
            {
                // maybe we should use the guid as url param to avoid the extra call for getting dictionary item
                var parentDictionary = Services.LocalizationService.GetDictionaryItemById(intId.Result);
                if (parentDictionary == null)
                    return nodes;

                nodes.AddRange(Services.LocalizationService.GetDictionaryItemChildren(parentDictionary.Key).ToList().OrderBy(ItemSort()).Select(
                    x => CreateTreeNode(
                        x.Id.ToInvariantString(),
                        id,
                        queryStrings,
                        x.ItemKey,
                        "icon-book-alt",
                        Services.LocalizationService.GetDictionaryItemChildren(x.Key).Any())));
            }

            return nodes;
        }

        /// <summary>
        /// Returns the menu structure for the node
        /// </summary>
        /// <param name="id">The id of the tree item</param>
        /// <param name="queryStrings">
        /// All of the query string parameters passed from jsTree
        /// </param>
        /// <returns></returns>
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            menu.Items.Add<ActionNew>(Services.TextService.Localize($"actions/{ActionNew.Instance.Alias}"));

            if (id != Constants.System.Root.ToInvariantString())
                menu.Items.Add<ActionDelete>(Services.TextService.Localize(
                    $"actions/{ActionDelete.Instance.Alias}"), true);

            menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(
                $"actions/{ActionRefresh.Instance.Alias}"), true);

            return menu;
        }
    }
}
