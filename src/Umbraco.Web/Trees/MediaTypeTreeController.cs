using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web.Search;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.MediaTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.MediaTypes, SortOrder = 1, TreeGroup = Constants.Trees.Groups.Settings)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public class MediaTypeTreeController : TreeController, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly IMediaTypeService _mediaTypeService;

        public MediaTypeTreeController(UmbracoTreeSearcher treeSearcher, IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper, IMediaTypeService mediaTypeService) : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _treeSearcher = treeSearcher;
            _mediaTypeService = mediaTypeService;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var intId = id.TryConvertTo<int>();
            if (intId == false) throw new InvalidOperationException("Id must be an integer");

            var nodes = new TreeNodeCollection();

            nodes.AddRange(
                Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.MediaTypeContainer)
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
            if (queryStrings["foldersonly"].IsNullOrWhiteSpace() == false && queryStrings["foldersonly"] == "1") return nodes;

            var mediaTypes = _mediaTypeService.GetAll();

            nodes.AddRange(
                Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.MediaType)
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

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.RootString)
            {
                // set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                // root actions
                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);
                menu.Items.Add(new RefreshNode(Services.TextService));
                return menu;
            }

            var container = Services.EntityService.Get(int.Parse(id), UmbracoObjectTypes.MediaTypeContainer);
            if (container != null)
            {
                // set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);

                menu.Items.Add(new MenuItem("rename", Services.TextService.Localize("actions", "rename"))
                {
                    Icon = "icon icon-edit"
                });

                if (container.HasChildren == false)
                {
                    // can delete doc type
                    menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);
                }
                menu.Items.Add(new RefreshNode(Services.TextService, true));
            }
            else
            {
                var ct = Services.MediaTypeService.Get(int.Parse(id));
                var parent = ct == null ? null : Services.MediaTypeService.Get(ct.ParentId);

                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);

                // no move action if this is a child doc type
                if (parent == null)
                {
                    menu.Items.Add<ActionMove>(Services.TextService, true, opensDialog: true);
                }

                menu.Items.Add<ActionCopy>(Services.TextService, opensDialog: true);
                if(ct.IsSystemMediaType() == false)
                {
                    menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);
                }
                menu.Items.Add(new RefreshNode(Services.TextService, true));

            }

            return menu;
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
            => _treeSearcher.EntitySearch(UmbracoObjectTypes.MediaType, query, pageSize, pageIndex, out totalFound, searchFrom);

    }
}
