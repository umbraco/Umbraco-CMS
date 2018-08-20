using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using AutoMapper;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Search;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.MediaTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.MediaTypes, null, sortOrder:5)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    [LegacyBaseTree(typeof(loadMediaTypes))]
    public class MediaTypeTreeController : TreeController, ISearchableTree
    {
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
                        var node = CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, "icon-folder", dt.HasChildren(), "");
                        node.Path = dt.Path;
                        node.NodeType = "container";
                        //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
                        node.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                        return node;
                    }));

            //if the request is for folders only then just return
            if (queryStrings["foldersonly"].IsNullOrWhiteSpace() == false && queryStrings["foldersonly"] == "1") return nodes;

            nodes.AddRange(
                Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.MediaType)
                    .OrderBy(entity => entity.Name)
                    .Select(dt =>
                    {
                        var node = CreateTreeNode(dt, Constants.ObjectTypes.MediaTypeGuid, id, queryStrings, "icon-thumbnails",
                            //NOTE: Since 7.4+ child type creation is enabled by a config option. It defaults to on, but can be disabled if we decide to. 
                            //We need this check to keep supporting sites where childs have already been created.
                            dt.HasChildren());

                        node.Path = dt.Path;
                        return node;
                    }));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            var enableInheritedMediaTypes = UmbracoConfig.For.UmbracoSettings().Content.EnableInheritedMediaTypes;

            if (id == Constants.System.Root.ToInvariantString())
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                // root actions              
                menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)));
                return menu;
            }

            var container = Services.EntityService.Get(int.Parse(id), UmbracoObjectTypes.MediaTypeContainer);
            if (container != null)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));

                menu.Items.Add(new MenuItem("rename", Services.TextService.Localize(String.Format("actions/{0}", "rename")))
                {
                    Icon = "icon icon-edit"
                });

                if (container.HasChildren() == false)
                {
                    //can delete doc type
                    menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
                }                
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), hasSeparator: true);

                
            }
            else
            {
                var ct = Services.ContentTypeService.GetMediaType(int.Parse(id));
                var parent = ct == null ? null : Services.ContentTypeService.GetMediaType(ct.ParentId);

                if (enableInheritedMediaTypes)
                {
                    menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));

                    //no move action if this is a child doc type
                    if (parent == null)
                    {
                        menu.Items.Add<ActionMove>(Services.TextService.Localize(string.Format("actions/{0}", ActionMove.Instance.Alias)), true);
                    }
                }
                else
                {
                    menu.Items.Add<ActionMove>(Services.TextService.Localize(string.Format("actions/{0}", ActionMove.Instance.Alias)));
                    //no move action if this is a child doc type
                    if (parent == null)
                    {
                        menu.Items.Add<ActionMove>(Services.TextService.Localize(string.Format("actions/{0}", ActionMove.Instance.Alias)), true);
                    }
                }

                menu.Items.Add<ActionCopy>(Services.TextService.Localize(string.Format("actions/{0}", ActionCopy.Instance.Alias)));
                menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
                if (enableInheritedMediaTypes)
                    menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);
            }

            return menu;
        }

        public IEnumerable<SearchResultItem> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            var results = Services.EntityService.GetPagedDescendantsFromRoot(UmbracoObjectTypes.MediaType, pageIndex, pageSize, out totalFound, filter: query);
            return Mapper.Map<IEnumerable<SearchResultItem>>(results);
        }
    }
}
