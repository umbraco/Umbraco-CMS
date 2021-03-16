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
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [Authorize(Policy = AuthorizationPolicies.TreeAccessRelationTypes)]
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
            IRelationService relationService,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory;
            _relationService = relationService;
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

            var relationType = _relationService.GetRelationTypeById(int.Parse(id));
            if (relationType == null) return menu;

            if (relationType.IsSystemRelationType() == false)
            {
                menu.Items.Add<ActionDelete>(LocalizedTextService);
            }

            return menu;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
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
