using System;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core;
using Umbraco.Web._Legacy.Actions;
using Umbraco.Core.Services;
using Umbraco.Core.Models;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.RelationTypes)]
    [Tree(Constants.Applications.Developer, Constants.Trees.RelationTypes, null, sortOrder: 4)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public class RelationTypeTreeController : TreeController
    {
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //Create the normal create action
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                //Since we haven't implemented anything for relationtypes in angular, this needs to be converted to 
                //use the legacy format
                .ConvertLegacyMenuItem(null, "initrelationTypes", queryStrings.GetValue<string>("application"));
                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);

                return menu;
            }

            var relationType = Services.RelationService.GetRelationTypeById(int.Parse(id));
            if (relationType == null) return new MenuItemCollection();

            //add delete option for all macros
            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias))
                //Since we haven't implemented anything for relationtypes in angular, this needs to be converted to 
                //use the legacy format
                .ConvertLegacyMenuItem(new UmbracoEntity
                {
                    Id = relationType.Id,
                    Level = 1,
                    ParentId = -1,
                    Name = relationType.Name
                }, "relationTypes", queryStrings.GetValue<string>("application"));

            return menu;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                nodes.AddRange(Services.RelationService
                .GetAllRelationTypes().Select(rt => CreateTreeNode(
                    rt.Id.ToString(),
                    id,
                    queryStrings,
                    rt.Name,
                    "icon-trafic",
                    false,
                    //TODO: Rebuild the macro editor in angular, then we dont need to have this at all (which is just a path to the legacy editor)
                    "/" + queryStrings.GetValue<string>("application") + "/framed/" +
                    Uri.EscapeDataString("/umbraco/developer/RelationTypes/EditRelationType.aspx?id=" + rt.Id)
                    )));
            }
            return nodes;
        }
    }
}
