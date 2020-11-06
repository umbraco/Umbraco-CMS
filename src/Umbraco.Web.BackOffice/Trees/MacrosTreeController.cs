using System.Linq;
using Microsoft.AspNetCore.Http;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Actions;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Macros)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Macros, TreeTitle = "Macros", SortOrder = 4, TreeGroup = Constants.Trees.Groups.Settings)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class MacrosTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly IMacroService  _macroService;

        public MacrosTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IMenuItemCollectionFactory menuItemCollectionFactory, IMacroService macroService) : base(localizedTextService, umbracoApiControllerTypeCollection)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory;
            _macroService = macroService;
        }

        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            //check if there are any macros
            root.HasChildren = _macroService.GetAll().Any();
            return root;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormCollection queryStrings)
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

        protected override MenuItemCollection GetMenuForNode(string id, FormCollection queryStrings)
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
