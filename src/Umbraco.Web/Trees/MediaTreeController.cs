using System;
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
    public class MediaTreeController : ContentTreeControllerBase
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var node = base.CreateRootNode(queryStrings);
            //if the user's start node is not default, then ensure the root doesn't have a menu
            if (Security.CurrentUser.StartMediaId != Constants.System.Root)
            {
                node.MenuUrl = "";
            }
            node.Name = ui.Text("sections", Constants.Trees.Media);
            return node;
        }

        protected override int RecycleBinId
        {
            get { return Constants.System.RecycleBinMedia; }
        }

        protected override bool RecycleBinSmells
        {
            get { return Services.MediaService.RecycleBinSmells(); }
        }

        protected override int UserStartNode
        {
            get { return Security.CurrentUser.StartMediaId; }
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
                e.Id.ToInvariantString(),
                parentId,
                queryStrings,
                e.Name,
                entity.ContentTypeIcon,
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
                //if the user's start node is not the root then ensure the root menu is empty/doesn't exist
                if (Security.CurrentUser.StartMediaId != Constants.System.Root)
                {
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
            //return a normal node menu:
            menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
            menu.Items.Add<ActionMove>(ui.Text("actions", ActionMove.Instance.Alias));
            menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));
            menu.Items.Add<ActionSort>(ui.Text("actions", ActionSort.Instance.Alias)).ConvertLegacyMenuItem(item, "media", "media");
            menu.Items.Add<ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

            //if the media item is in the recycle bin, don't have a default menu, just show the regular menu
            if (item.Path.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Contains(RecycleBinId.ToInvariantString()))
            {
                menu.DefaultMenuAlias = null;
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
            var media = Services.MediaService.GetById(int.Parse(id));
            if (media == null)
            {
                return false;
            }
            return Security.CurrentUser.HasPathAccess(media);
        }
    }
}