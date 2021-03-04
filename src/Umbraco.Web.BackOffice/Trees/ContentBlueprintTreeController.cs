using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    /// <summary>
    /// The content blueprint tree controller
    /// </summary>
    /// <remarks>
    /// This authorizes based on access to the content section even though it exists in the settings
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
    [Tree(Constants.Applications.Settings, Constants.Trees.ContentBlueprints, SortOrder = 12, TreeGroup = Constants.Trees.Groups.Settings)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class ContentBlueprintTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IEntityService _entityService;

        public ContentBlueprintTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IContentService contentService,
            IContentTypeService contentTypeService,
            IEntityService entityService,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory ?? throw new ArgumentNullException(nameof(menuItemCollectionFactory));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var rootResult = base.CreateRootNode(queryStrings);
            if (!(rootResult.Result is null))
            {
                return rootResult;
            }
            var root = rootResult.Value;

            //this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{Constants.Applications.Settings}/{Constants.Trees.ContentBlueprints}/intro";

            //check if there are any content blueprints
            root.HasChildren = _contentService.GetBlueprintsForContentTypes().Any();

            return root;
        }
        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            //get all blueprints
            var entities = _entityService.GetChildren(Constants.System.Root, UmbracoObjectTypes.DocumentBlueprint).ToArray();

            //check if we're rendering the root in which case we'll render the content types that have blueprints
            if (id == Constants.System.RootString)
            {
                //get all blueprint content types
                var contentTypeAliases = entities.Select(x => ((IContentEntitySlim) x).ContentTypeAlias).Distinct();
                //get the ids
                var contentTypeIds = _contentTypeService.GetAllContentTypeIds(contentTypeAliases.ToArray()).ToArray();

                //now get the entities ... it's a bit round about but still smaller queries than getting all document types
                var docTypeEntities = contentTypeIds.Length == 0
                    ? new IUmbracoEntity[0]
                    : _entityService.GetAll(UmbracoObjectTypes.DocumentType, contentTypeIds).ToArray();

                nodes.AddRange(docTypeEntities
                    .Select(entity =>
                    {
                        var treeNode = CreateTreeNode(entity, Constants.ObjectTypes.DocumentBlueprint, id, queryStrings, Constants.Icons.ContentType, true);
                        treeNode.Path = $"-1,{entity.Id}";
                        treeNode.NodeType = "document-type-blueprints";
                        // TODO: This isn't the best way to ensure a no operation process for clicking a node but it works for now.
                        treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                        return treeNode;
                    }));

                return nodes;
            }

            var intId = id.TryConvertTo<int>();
            //Get the content type
            var ct = _contentTypeService.Get(intId.Result);
            if (ct == null) return nodes;

            var blueprintsForDocType = entities.Where(x => ct.Alias == ((IContentEntitySlim) x).ContentTypeAlias);
            nodes.AddRange(blueprintsForDocType
                .Select(entity =>
                {
                    var treeNode = CreateTreeNode(entity, Constants.ObjectTypes.DocumentBlueprint, id, queryStrings, "icon-blueprint", false);
                    treeNode.Path = $"-1,{ct.Id},{entity.Id}";
                    return treeNode;
                }));

            return nodes;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            if (id == Constants.System.RootString)
            {
                // root actions
                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                return menu;
            }
            var cte = _entityService.Get(int.Parse(id), UmbracoObjectTypes.DocumentType);
            //only refresh & create if it's a content type
            if (cte != null)
            {
                var ct = _contentTypeService.Get(cte.Id);
                var createItem = menu.Items.Add<ActionCreateBlueprintFromContent>(LocalizedTextService, opensDialog: true);
                createItem.NavigateToRoute("/settings/contentBlueprints/edit/-1?create=true&doctype=" + ct.Alias);

                menu.Items.Add(new RefreshNode(LocalizedTextService, true));

                return menu;
            }

            menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);

            return menu;
        }
    }
}
