using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMacros)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Macros, TreeTitle = "Macros", SortOrder = 4, TreeGroup = Constants.Trees.Groups.Settings)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class MacrosTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly IMacroService  _macroService;

        public MacrosTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IMenuItemCollectionFactory menuItemCollectionFactory, IMacroService macroService, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory;
            _macroService = macroService;
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var rootResult = base.CreateRootNode(queryStrings);
            if (!(rootResult.Result is null))
            {
                return rootResult;
            }
            var root = rootResult.Value;

            //check if there are any macros
            root.HasChildren = _macroService.GetAll().Any();
            return root;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.RootString)
            {
                foreach (var macro in _macroService.GetAll().OrderBy(m => m.Name))
                {
                    nodes.Add(CreateTreeNode(
                        macro.Id.ToString(),
                        id,
                        queryStrings,
                        macro.Name,
                        Constants.Icons.Macro,
                        false));
                }
            }

            return nodes;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            if (id == Constants.System.RootString)
            {
                //Create the normal create action
                menu.Items.Add<ActionNew>(LocalizedTextService);

                //refresh action
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));

                return menu;
            }

            var macro = _macroService.GetById(int.Parse(id));
            if (macro == null) return menu;

            //add delete option for all macros
            menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);

            return menu;
        }
    }
}
