using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Search;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.DocumentTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.DocumentTypes, SortOrder = 0, TreeGroup = Constants.Trees.Groups.Settings)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public class ContentTypeTreeController : TreeController, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher;

        public ContentTypeTreeController(UmbracoTreeSearcher treeSearcher, IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper) : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _treeSearcher = treeSearcher;
        }

        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            //check if there are any types
            root.HasChildren = Services.ContentTypeService.GetAll().Any();
            return root;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var intId = id.TryConvertTo<int>();
            if (intId == false) throw new InvalidOperationException("Id must be an integer");

            var nodes = new TreeNodeCollection();

            nodes.AddRange(
                Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.DocumentTypeContainer)
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

            //if the request is for folders only then just return
            if (queryStrings["foldersonly"].IsNullOrWhiteSpace() == false && queryStrings["foldersonly"] == "1") return nodes;

            var children = Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.DocumentType).ToArray();
            var contentTypes = Services.ContentTypeService.GetAll(children.Select(c => c.Id).ToArray()).ToDictionary(c => c.Id);
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

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.RootString)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                // root actions
                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);
                menu.Items.Add(new MenuItem("importDocumentType", Services.TextService)
                {
                    Icon = "page-up",
                    SeparatorBefore = true,
                    OpensDialog = true
                });
                menu.Items.Add(new RefreshNode(Services.TextService, true));

                return menu;
            }

            var container = Services.EntityService.Get(int.Parse(id), UmbracoObjectTypes.DocumentTypeContainer);
            if (container != null)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);

                menu.Items.Add(new MenuItem("rename", Services.TextService)
                {
                    Icon = "icon icon-edit"
                });

                if (container.HasChildren == false)
                {
                    //can delete doc type
                    menu.Items.Add<ActionDelete>(Services.TextService, true, opensDialog: true);
                }
                menu.Items.Add(new RefreshNode(Services.TextService, true));
            }
            else
            {
                var ct = Services.ContentTypeService.Get(int.Parse(id));
                var parent = ct == null ? null : Services.ContentTypeService.Get(ct.ParentId);

                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);
                //no move action if this is a child doc type
                if (parent == null)
                {
                    menu.Items.Add<ActionMove>(Services.TextService, true, opensDialog: true);
                }
                menu.Items.Add<ActionCopy>(Services.TextService, opensDialog: true);
                menu.Items.Add(new MenuItem("export", Services.TextService)
                {
                    Icon = "download-alt",
                    SeparatorBefore = true,
                    OpensDialog = true
                });
                menu.Items.Add<ActionDelete>(Services.TextService, true, opensDialog: true);
                menu.Items.Add(new RefreshNode(Services.TextService, true));

            }

            return menu;
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
            => _treeSearcher.EntitySearch(UmbracoObjectTypes.DocumentType, query, pageSize, pageIndex, out totalFound, searchFrom);

    }
}
