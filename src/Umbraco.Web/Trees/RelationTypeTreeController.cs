﻿using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.RelationTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.RelationTypes, null, sortOrder: 5)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree(TreeGroup = Constants.Trees.Groups.Settings)]
    public class RelationTypeTreeController : TreeController
    {
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //Create the normal create action
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.ActionAlias));

                //refresh action
                menu.Items.Add(new RefreshNode(Services.TextService, true));

                return menu;
            }

            var relationType = Services.RelationService.GetRelationTypeById(int.Parse(id));
            if (relationType == null) return new MenuItemCollection();

            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.ActionAlias));

            return menu;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                nodes.AddRange(Services.RelationService.GetAllRelationTypes()
                                       .Select(rt => CreateTreeNode(rt.Id.ToString(), id, queryStrings, rt.Name,
                                                                    "icon-trafic", false)));
            }
            return nodes;
        }
    }
}
