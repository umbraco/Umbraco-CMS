using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Search;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Web.Common.Authorization;

namespace Umbraco.Web.BackOffice.Trees
{
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.MediaTypes, SortOrder = 1, TreeGroup = Constants.Trees.Groups.Settings)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class MediaTypeTreeController : TreeController, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IEntityService  _entityService;

        public MediaTypeTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, UmbracoTreeSearcher treeSearcher, IMenuItemCollectionFactory menuItemCollectionFactory, IMediaTypeService mediaTypeService, IEntityService entityService) : base(localizedTextService, umbracoApiControllerTypeCollection)
        {
            _treeSearcher = treeSearcher;
            _menuItemCollectionFactory = menuItemCollectionFactory;
            _mediaTypeService = mediaTypeService;
            _entityService = entityService;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormCollection queryStrings)
        {
            var intId = id.TryConvertTo<int>();
            if (intId == false) throw new InvalidOperationException("Id must be an integer");

            var nodes = new TreeNodeCollection();

            nodes.AddRange(
                _entityService.GetChildren(intId.Result, UmbracoObjectTypes.MediaTypeContainer)
                    .OrderBy(entity => entity.Name)
                    .Select(dt =>
                    {
                        var node = CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, "icon-folder", dt.HasChildren, "");
                        node.Path = dt.Path;
                        node.NodeType = "container";
                        // TODO: This isn't the best way to ensure a no operation process for clicking a node but it works for now.
                        node.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                        return node;
                    }));

            // if the request is for folders only then just return
            if (queryStrings["foldersonly"].ToString().IsNullOrWhiteSpace() == false && queryStrings["foldersonly"].ToString() == "1") return nodes;

            var mediaTypes = _mediaTypeService.GetAll();

            nodes.AddRange(
                _entityService.GetChildren(intId.Result, UmbracoObjectTypes.MediaType)
                    .OrderBy(entity => entity.Name)
                    .Select(dt =>
                    {
                        // since 7.4+ child type creation is enabled by a config option. It defaults to on, but can be disabled if we decide to.
                        // need this check to keep supporting sites where children have already been created.
                        var hasChildren = dt.HasChildren;
                        var mt = mediaTypes.FirstOrDefault(x => x.Id == dt.Id);
                        var node = CreateTreeNode(dt, Constants.ObjectTypes.MediaType, id, queryStrings,  mt?.Icon ?? Constants.Icons.MediaType, hasChildren);

                        node.Path = dt.Path;
                        return node;
                    }));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            if (id == Constants.System.RootString)
            {
                // set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                // root actions
                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);
                menu.Items.Add(new RefreshNode(LocalizedTextService));
                return menu;
            }

            var container = _entityService.Get(int.Parse(id), UmbracoObjectTypes.MediaTypeContainer);
            if (container != null)
            {
                // set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);

                menu.Items.Add(new MenuItem("rename", LocalizedTextService.Localize("actions/rename"))
                {
                    Icon = "icon icon-edit"
                });

                if (container.HasChildren == false)
                {
                    // can delete doc type
                    menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);
                }
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
            }
            else
            {
                var ct = _mediaTypeService.Get(int.Parse(id));
                var parent = ct == null ? null : _mediaTypeService.Get(ct.ParentId);

                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);

                // no move action if this is a child doc type
                if (parent == null)
                {
                    menu.Items.Add<ActionMove>(LocalizedTextService, true, opensDialog: true);
                }

                menu.Items.Add<ActionCopy>(LocalizedTextService, opensDialog: true);
                if(ct.IsSystemMediaType() == false)
                {
                    menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);
                }
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            }

            return menu;
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
            => _treeSearcher.EntitySearch(UmbracoObjectTypes.MediaType, query, pageSize, pageIndex, out totalFound, searchFrom);

    }
}
