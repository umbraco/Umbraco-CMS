using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web._Legacy.Actions;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Search;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    //We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
    // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
    [UmbracoApplicationAuthorize(
        Constants.Applications.Content,
        Constants.Applications.Media,
        Constants.Applications.Settings,
        Constants.Applications.Packages,
        Constants.Applications.Members)]
    [Tree(Constants.Applications.Media, Constants.Trees.Media)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    [SearchableTree("searchResultFormatter", "configureMediaResult")]
    public class MediaTreeController : ContentTreeControllerBase, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher = new UmbracoTreeSearcher();

        protected override int RecycleBinId => Constants.System.RecycleBinMedia;

        protected override bool RecycleBinSmells => Services.MediaService.RecycleBinSmells();

        private int[] _userStartNodes;
        protected override int[] UserStartNodes
            => _userStartNodes ?? (_userStartNodes = Security.CurrentUser.CalculateMediaStartNodeIds(Services.EntityService));

        /// <summary>
        /// Creates a tree node for a content item based on an UmbracoEntity
        /// </summary>
        /// <param name="e"></param>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode GetSingleTreeNode(IEntitySlim entity, string parentId, FormDataCollection queryStrings)
        {
            //Special check to see if it ia a container, if so then we'll hide children.
            var isContainer = entity.IsContainer; // && (queryStrings.Get("isDialog") != "true");

            var node = CreateTreeNode(
                entity,
                Constants.ObjectTypes.Media,
                parentId,
                queryStrings,
                entity.HasChildren && !isContainer);

            // entity is either a container, or a media
            if (isContainer)
            {
                node.SetContainerStyle();
                node.AdditionalData.Add("isContainer", true);
            }
            else
            {
                var contentEntity = (IContentEntitySlim) entity;
                node.AdditionalData.Add("contentType", contentEntity.ContentTypeAlias);
            }

            return node;
        }

        protected override MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //set the default
            menu.DefaultMenuAlias = ActionNew.Instance.Alias;

            if (id == Constants.System.Root.ToInvariantString())
            {
                // if the user's start node is not the root then the only menu item to display is refresh
                if (UserStartNodes.Contains(Constants.System.Root) == false)
                {
                    menu.Items.Add<RefreshNode, ActionRefresh>(
                        Services.TextService.Localize(string.Concat("actions/", ActionRefresh.Instance.Alias)),
                        true);
                    return menu;
                }

                // root actions
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias));
                menu.Items.Add<ActionSort>(Services.TextService.Localize("actions", ActionSort.Instance.Alias), true);
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }

            int iid;
            if (int.TryParse(id, out iid) == false)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var item = Services.EntityService.Get(iid, UmbracoObjectTypes.Media);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //if the user has no path access for this node, all they can do is refresh
            if (Security.CurrentUser.HasPathAccess(item, Services.EntityService, RecycleBinId) == false)
            {
                menu.Items.Add<RefreshNode, ActionRefresh>(
                    Services.TextService.Localize(string.Concat("actions/", ActionRefresh.Instance.Alias)),
                    true);
                return menu;
            }

            //return a normal node menu:
            menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias));
            menu.Items.Add<ActionMove>(Services.TextService.Localize("actions", ActionMove.Instance.Alias));
            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias));
            menu.Items.Add<ActionSort>(Services.TextService.Localize("actions", ActionSort.Instance.Alias));
            menu.Items.Add<ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);

            //if the media item is in the recycle bin, don't have a default menu, just show the regular menu
            if (item.Path.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Contains(RecycleBinId.ToInvariantString()))
            {
                menu.DefaultMenuAlias = null;
                menu.Items.Insert(2, new MenuItem(ActionRestore.Instance, Services.TextService.Localize("actions", ActionRestore.Instance.Alias)));
            }
            else
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;
            }

            return menu;
        }

        protected override UmbracoObjectTypes UmbracoObjectType => UmbracoObjectTypes.Media;

        /// <summary>
        /// Returns true or false if the current user has access to the node based on the user's allowed start node (path) access
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override bool HasPathAccess(string id, FormDataCollection queryStrings)
        {
            var entity = GetEntityFromId(id);

            return HasPathAccess(entity, queryStrings);
        }

        public IEnumerable<SearchResultItem> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            return _treeSearcher.ExamineSearch(Umbraco, query, UmbracoEntityTypes.Media, pageSize, pageIndex, out totalFound, searchFrom);
        }
    }
}
