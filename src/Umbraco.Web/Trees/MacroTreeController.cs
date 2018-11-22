using System;
using System.Net.Http.Formatting;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using System.Linq;
using umbraco.BusinessLogic.Actions;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Services;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Macros)]
    [Tree(Constants.Applications.Developer, Constants.Trees.Macros, null, sortOrder: 2)]
    [LegacyBaseTree(typeof(loadMacros))]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class MacroTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var intId = id.TryConvertTo<int>();
            if (intId == false) throw new InvalidOperationException("Id must be an integer");

            var nodes = new TreeNodeCollection();
            
            nodes.AddRange(
                Services.MacroService.GetAll()
                    .OrderBy(entity => entity.Name)
                    .Select(macro =>
                    {
                        var node = CreateTreeNode(macro.Id.ToInvariantString(), id, queryStrings, macro.Name, "icon-settings-alt", false);
                        node.Path = "-1," + macro.Id;
                        node.AssignLegacyJsCallback("javascript:UmbClientMgr.contentFrame('developer/macros/editMacro.aspx?macroID=" + macro.Id + "');");
                        return node;
                    }));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                // root actions
                menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)))
                    .ConvertLegacyMenuItem(null, Constants.Trees.Macros, queryStrings.GetValue<string>("application"));
                
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }

            //TODO: This is all hacky ... don't have time to convert the tree, views and dialogs over properly so we'll keep using the legacy dialogs
            var menuItem = menu.Items.Add(ActionDelete.Instance, Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
            var legacyConfirmView = LegacyTreeDataConverter.GetLegacyConfirmView(ActionDelete.Instance);
            if (legacyConfirmView == false)
                throw new InvalidOperationException("Could not resolve the confirmation view for the legacy action " + ActionDelete.Instance.Alias);
            menuItem.LaunchDialogView(
                legacyConfirmView.Result,
                Services.TextService.Localize(string.Format("general/{0}", ActionDelete.Instance.Alias)));

            return menu;
        }
    }
}