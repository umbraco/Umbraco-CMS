using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{

    // We are allowed to see the dictionary tree, if we are allowed to manage templates, such that se can use the
    // dictionary items in templates, even when we dont have authorization to manage the dictionary items
    [Authorize(Policy = AuthorizationPolicies.TreeAccessDictionaryOrTemplates)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    [Tree(Constants.Applications.Translation, Constants.Trees.Dictionary, TreeGroup = Constants.Trees.Groups.Settings)]
    public class DictionaryTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly ILocalizationService _localizationService;

        public DictionaryTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IMenuItemCollectionFactory menuItemCollectionFactory, ILocalizationService localizationService, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory;
            _localizationService = localizationService;
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var rootResult = base.CreateRootNode(queryStrings);
            if (!(rootResult.Result is null))
            {
                return rootResult;
            }
            var root = rootResult.Value;

            // the default section is settings, falling back to this if we can't
            // figure out where we are from the querystring parameters
            var section = Constants.Applications.Translation;
            if (!queryStrings["application"].ToString().IsNullOrWhiteSpace())
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
        /// We are allowing an arbitrary number of query strings to be passed in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            var intId = id.TryConvertTo<int>();
            if (intId == false)
                throw new InvalidOperationException("Id must be an integer");

            var nodes = new TreeNodeCollection();

            Func<IDictionaryItem, string> ItemSort() => item => item.ItemKey;

            if (id == Constants.System.RootString)
            {
                nodes.AddRange(
                    _localizationService.GetRootDictionaryItems().OrderBy(ItemSort()).Select(
                        x => CreateTreeNode(
                            x.Id.ToInvariantString(),
                            id,
                            queryStrings,
                            x.ItemKey,
                            Constants.Icons.Dictionary,
                            _localizationService.GetDictionaryItemChildren(x.Key).Any())));
            }
            else
            {
                // maybe we should use the guid as URL param to avoid the extra call for getting dictionary item
                var parentDictionary = _localizationService.GetDictionaryItemById(intId.Result);
                if (parentDictionary == null)
                    return nodes;

                nodes.AddRange(_localizationService.GetDictionaryItemChildren(parentDictionary.Key).ToList().OrderBy(ItemSort()).Select(
                    x => CreateTreeNode(
                        x.Id.ToInvariantString(),
                        id,
                        queryStrings,
                        x.ItemKey,
                        Constants.Icons.Dictionary,
                        _localizationService.GetDictionaryItemChildren(x.Key).Any())));
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
        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);

            if (id != Constants.System.RootString)
                menu.Items.Add<ActionDelete>(LocalizedTextService, true, opensDialog: true);

            menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            return menu;
        }
    }
}
