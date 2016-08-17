using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Web._Legacy.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Macros)]
    [Tree(Constants.Applications.Developer, Constants.Trees.Macros, "Macros", sortOrder: 2)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class MacrosTreeController : TreeController
    {
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
                        false,
                        //TODO: Rebuild the macro editor in angular, then we dont need to have this at all (which is just a path to the legacy editor)
                        "/" + queryStrings.GetValue<string>("application") + "/framed/" +
                        Uri.EscapeDataString("/umbraco/developer/macros/editMacro.aspx?macroID=" + macro.Id)));
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
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                    //Since we haven't implemented anything for macros in angular, this needs to be converted to 
                    //use the legacy format
                    .ConvertLegacyMenuItem(null, "initmacros", queryStrings.GetValue<string>("application"));

                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);

                return menu;
            }


            var macro = Services.MacroService.GetById(int.Parse(id));
            if (macro == null) return new MenuItemCollection();

            //add delete option for all macros
            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias))
                //Since we haven't implemented anything for macros in angular, this needs to be converted to 
                //use the legacy format
                .ConvertLegacyMenuItem(new UmbracoEntity
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