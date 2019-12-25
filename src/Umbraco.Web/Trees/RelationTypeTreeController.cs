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
    [Tree(Constants.Applications.Settings, Constants.Trees.RelationTypes, SortOrder = 5, TreeGroup = Constants.Trees.Groups.Settings)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public class RelationTypeTreeController : TreeController
    {
        private readonly IRelationService _relationService;

        public RelationTypeTreeController(IRelationService relationService)
        {
            _relationService = relationService;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            //TODO: Do not allow deleting built in types

            var menu = new MenuItemCollection();

            if (id == Constants.System.RootString)
            {
                //Create the normal create action
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.ActionAlias));

                //refresh action
                menu.Items.Add(new RefreshNode(Services.TextService, true));

                return menu;
            }

            var relationType = _relationService.GetRelationTypeById(int.Parse(id));
            if (relationType == null) return new MenuItemCollection();

            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.ActionAlias));

            return menu;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.RootString)
            {
                nodes.AddRange(_relationService.GetAllRelationTypes()
                    .Select(rt => CreateTreeNode(rt.Id.ToString(), id, queryStrings, rt.Name,
                        "icon-trafic", false)));
            }

            return nodes;
        }
    }
}
