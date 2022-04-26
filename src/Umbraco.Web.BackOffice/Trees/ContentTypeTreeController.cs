using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.DocumentTypes, SortOrder = 0, TreeGroup = Constants.Trees.Groups.Settings)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class ContentTypeTreeController : TreeController, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly IContentTypeService _contentTypeService;
        private readonly IEntityService _entityService;

        public ContentTypeTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, UmbracoTreeSearcher treeSearcher, IMenuItemCollectionFactory menuItemCollectionFactory, IContentTypeService contentTypeService, IEntityService entityService, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _treeSearcher = treeSearcher;
            _menuItemCollectionFactory = menuItemCollectionFactory;
            _contentTypeService = contentTypeService;
            _entityService = entityService;
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var rootResult = base.CreateRootNode(queryStrings);
            if (!(rootResult.Result is null))
            {
                return rootResult;
            }
            var root = rootResult.Value;
            //check if there are any types
            root.HasChildren = _contentTypeService.GetAll().Any();
            return root;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            if (!int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
            {
                throw new InvalidOperationException("Id must be an integer");
            }

            var nodes = new TreeNodeCollection();

            nodes.AddRange(
                _entityService.GetChildren(intId, UmbracoObjectTypes.DocumentTypeContainer)
                    .OrderBy(entity => entity.Name)
                    .Select(dt =>
                    {
                        var node = CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, Constants.Icons.Folder, dt.HasChildren, "");
                        node.Path = dt.Path;
                        node.NodeType = "container";
                        // TODO: This isn't the best way to ensure a no operation process for clicking a node but it works for now.
                        node.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                        return node;
                    }));

            //if the request is for folders only then just return
            if (queryStrings["foldersonly"].ToString().IsNullOrWhiteSpace() == false && queryStrings["foldersonly"] == "1")
                return nodes;

            var children = _entityService.GetChildren(intId, UmbracoObjectTypes.DocumentType).ToArray();
            var contentTypes = _contentTypeService.GetAll(children.Select(c => c.Id).ToArray()).ToDictionary(c => c.Id);
            nodes.AddRange(
                children
                    .OrderBy(entity => entity.Name)
                    .Select(dt =>
                    {
                        // get the content type here so we can get the icon from it to use when we create the tree node
                        // and we can enrich the result with content type data that's not available in the entity service output
                        var contentType = contentTypes[dt.Id];

                        // since 7.4+ child type creation is enabled by a config option. It defaults to on, but can be disabled if we decide to.
                        // need this check to keep supporting sites where children have already been created.
                        var hasChildren = dt.HasChildren;
                        var node = CreateTreeNode(dt, Constants.ObjectTypes.DocumentType, id, queryStrings, contentType?.Icon ?? Constants.Icons.ContentType, hasChildren);

                        node.Path = dt.Path;

                        // now we can enrich the result with content type data that's not available in the entity service output
                        node.Alias = contentType.Alias;
                        node.AdditionalData["isElement"] = contentType.IsElement;

                        return node;
                    }));

            return nodes;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();
            
            if (id == Constants.System.RootString)
            {
                // set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                // root actions
                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);
                menu.Items.Add(new MenuItem("importdocumenttype", LocalizedTextService)
                {
                    Icon = "icon-page-up",
                    SeparatorBefore = true,
                    OpensDialog = true,
                    UseLegacyIcon = false,
                });

                menu.Items.Add(new RefreshNode(LocalizedTextService, true));

                return menu;
            }

            var container = _entityService.Get(int.Parse(id, CultureInfo.InvariantCulture), UmbracoObjectTypes.DocumentTypeContainer);
            if (container != null)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);

                menu.Items.Add(new MenuItem("rename", LocalizedTextService)
                {
                    Icon = "icon-edit",
                    UseLegacyIcon = false,
                });

                if (container.HasChildren == false)
                {
                    // can delete doc type
                    menu.Items.Add<ActionDelete>(LocalizedTextService, true, opensDialog: true, useLegacyIcon: false);
                }

                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
            }
            else
            {
                var ct = _contentTypeService.Get(int.Parse(id, CultureInfo.InvariantCulture));
                var parent = ct == null ? null : _contentTypeService.Get(ct.ParentId);

                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);

                // no move action if this is a child doc type
                if (parent == null)
                {
                    menu.Items.Add<ActionMove>(LocalizedTextService, true, opensDialog: true, useLegacyIcon: false);
                }

                menu.Items.Add<ActionCopy>(LocalizedTextService, opensDialog: true);
                menu.Items.Add(new MenuItem("export", LocalizedTextService)
                {
                    Icon = "icon-download-alt",
                    SeparatorBefore = true,
                    OpensDialog = true,
                    UseLegacyIcon = false,
                });

                menu.Items.Add<ActionDelete>(LocalizedTextService, true, opensDialog: true, useLegacyIcon: false);
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
            }

            return menu;
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
            => _treeSearcher.EntitySearch(UmbracoObjectTypes.DocumentType, query, pageSize, pageIndex, out totalFound, searchFrom);

    }
}
