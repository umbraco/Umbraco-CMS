using System.Linq;
using Microsoft.AspNetCore.Http;
using Umbraco.Web.Models.Trees;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.BackOffice.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.RelationTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.RelationTypes, SortOrder = 5, TreeGroup = Constants.Trees.Groups.Settings)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class RelationTypeTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly IRelationService _relationService;

        public RelationTypeTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IRelationService relationService)
            : base(localizedTextService, umbracoApiControllerTypeCollection)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory;
            _relationService = relationService;
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

            var relationType = _relationService.GetRelationTypeById(int.Parse(id));
            if (relationType == null) return menu;

            if (relationType.IsSystemRelationType() == false)
            {
                menu.Items.Add<ActionDelete>(LocalizedTextService);
            }

            return menu;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormCollection queryStrings)
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
