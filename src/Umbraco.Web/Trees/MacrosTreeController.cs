using System;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Models.Entities;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Macros)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Macros, "Macros", sortOrder: 4)]
    [PluginController("UmbracoTrees")]
    [CoreTree(TreeGroup = Constants.Trees.Groups.Settings)]
    public class MacrosTreeController : TreeController
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            //check if there are any macros
            root.HasChildren = Services.MacroService.GetAll().Any();
            return root;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                foreach (var macro in Services.MacroService.GetAll())
                {
                    nodes.Add(CreateTreeNode(
                        macro.Id.ToString(),
                        id,
                        queryStrings,
                        macro.Name,
                        "icon-settings-alt",
                        false));
                }
            }

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //Create the normal create action
                menu.Items.Add<ActionNew>(Services.TextService);
                    
                //refresh action
                menu.Items.Add(new RefreshNode(Services.TextService, true));

                return menu;
            }

            var macro = Services.MacroService.GetById(int.Parse(id));
            if (macro == null) return new MenuItemCollection();

            //add delete option for all macros
            menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true)
                //Since we haven't implemented anything for macros in angular, this needs to be converted to
                //use the legacy format
                .ConvertLegacyMenuItem(new EntitySlim
                {
                    Id = macro.Id,
                    Level = 1,
                    ParentId = -1,
                    Name = macro.Name
                }, "macros", queryStrings.GetValue<string>("application"));

            return menu;
        }
    }
}
