using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using umbraco.BusinessLogic.Actions;
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
        Constants.Applications.Developer,
        Constants.Applications.Members)]
    [LegacyBaseTree(typeof(loadMedia))]
    [Tree(Constants.Applications.Media, Constants.Trees.Media)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    [SearchableTree("searchResultFormatter", "configureMediaResult")]
    public class MediaTreeController : ContentTreeControllerBase, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher = new UmbracoTreeSearcher();
        
        protected override int RecycleBinId
        {
            get { return Constants.System.RecycleBinMedia; }
        }

        protected override bool RecycleBinSmells
        {
            get { return Services.MediaService.RecycleBinSmells(); }
        }

        private int[] _userStartNodes;
        protected override int[] UserStartNodes
        {
            get { return _userStartNodes ?? (_userStartNodes = Security.CurrentUser.CalculateMediaStartNodeIds(Services.EntityService)); }
        }

        /// <summary>
        /// Creates a tree node for a content item based on an UmbracoEntity
        /// </summary>
        /// <param name="e"></param>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode GetSingleTreeNode(IUmbracoEntity e, string parentId, FormDataCollection queryStrings)
        {
            var entity = (UmbracoEntity)e;

            //Special check to see if it ia a container, if so then we'll hide children.
            var isContainer = e.IsContainer(); // && (queryStrings.Get("isDialog") != "true");

            var node = CreateTreeNode(
                entity,
                Constants.ObjectTypes.MediaGuid,
                parentId,
                queryStrings,
                entity.HasChildren && (isContainer == false));

            node.AdditionalData.Add("contentType", entity.ContentTypeAlias);

            if (isContainer)
            {
                node.SetContainerStyle();
                node.AdditionalData.Add("isContainer", true);
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
                menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
                menu.Items.Add<ActionSort>(ui.Text("actions", ActionSort.Instance.Alias), true).ConvertLegacyMenuItem(null, "media", "media");
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
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

            //if the media item is in the recycle bin, we don't have a default menu and we need to show a limited menu
            if (item.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(RecycleBinId.ToInvariantString()))
            {
                menu.Items.Add<ActionRestore>(ui.Text("actions", ActionRestore.Instance.Alias));
                menu.Items.Add<ActionMove>(ui.Text("actions", ActionMove.Instance.Alias));
                menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

                menu.DefaultMenuAlias = null;
            }
            else
            {
                //return a normal node menu:
                menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
                menu.Items.Add<ActionMove>(ui.Text("actions", ActionMove.Instance.Alias));
                menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));
                menu.Items.Add<ActionSort>(ui.Text("actions", ActionSort.Instance.Alias)).ConvertLegacyMenuItem(item, "media", "media");
                menu.Items.Add<ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;
            }

            return menu;
        }

        protected override UmbracoObjectTypes UmbracoObjectType
        {
            get { return UmbracoObjectTypes.Media; }
        }

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

        internal override IEnumerable<IUmbracoEntity> GetChildrenFromEntityService(int entityId)
            // Not pretty having to cast the service, but it is the only way to get to use an internal method that we
            // do not want to make public on the interface. Unfortunately also prevents this from being unit tested.
            // See this issue for details on why we need this:
            // https://github.com/umbraco/Umbraco-CMS/issues/3457
            => ((EntityService)Services.EntityService).GetMediaChildrenWithoutPropertyData(entityId).ToList();        
    }
}
